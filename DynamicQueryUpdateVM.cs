using DynamicData;
using ReactiveDynamicData.Diff;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReactiveDynamicData
{
    public static class ObservableExtensions
    {
        public static IObservable<IChangeSet<string>> CalculateChanges(this IObservable<string[]> source)
        {
            return source
                .Scan((Array.Empty<string>(), (IChangeSet<string>)null), (accumulator, newQuery) =>
           {
               var previousQuery = accumulator.Item1;

               // NOTE; Clear the list on new empty queries - optimization.
               if (newQuery.Length == 0)
               {
                   return (newQuery, new ChangeSet<string>() { new Change<string>(ListChangeReason.Clear, previousQuery) });
               }

               var diffs = ListDiff.Compare(previousQuery, newQuery, EqualityComparer<string>.Default);
               var changeSet = new ChangeSet<string>();
               var changeStartIdx = 0;
               foreach (var diff in diffs)
               {
                   Change<string> change = null;
                   switch (diff.Operation)
                   {
                       case Operation.Equal:
                           {
                           }
                           break;
                       case Operation.Insert:
                           {
                               if (diff.Items.Count > 1)
                               {
                                   change = new Change<string>(ListChangeReason.AddRange, diff.Items, changeStartIdx);
                               }
                               else
                               {
                                   change = new Change<string>(ListChangeReason.Add, diff.Items.First(), changeStartIdx);
                               }
                           }
                           break;
                       case Operation.Delete:
                           {
                               if (diff.Items.Count > 1)
                               {
                                   change = new Change<string>(ListChangeReason.RemoveRange, diff.Items, changeStartIdx);
                               }
                               else
                               {
                                   change = new Change<string>(ListChangeReason.Remove, diff.Items.First(), changeStartIdx);
                               }
                           }
                           break;
                   }

                   changeStartIdx += diff.Items.Count;
                   if (change != null)
                   {
                       changeSet.Add(change);
                   }
               }

               return (newQuery, changeSet);
           })
                .Select(x => x.Item2);
        }
    }

    public class DynamicQueryUpdateVM : ReactiveObject
    {
        private string _searchInput;
        public string SearchInput
        {
            get => _searchInput;
            set => this.RaiseAndSetIfChanged(ref _searchInput, value);
        }

        private readonly ObservableAsPropertyHelper<IEnumerable<OptionsModel>> _searchResults;
        public IEnumerable<OptionsModel> SearchResults => _searchResults.Value;

        public DynamicQueryUpdateVM()
        {
            _searchResults = this
                .WhenAnyValue(@this => @this.SearchInput)
                .Throttle(TimeSpan.FromMilliseconds(800))
                .Select(term => term?.Trim() ?? string.Empty)
                .DistinctUntilChanged()
                .Select(term => term.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                .CalculateChanges()
                .ToCollection()
                .SelectMany(DoThing)
                .ObserveOnDispatcher()
                .ToProperty(this, @this => @this.SearchResults);
        }

        private async Task<IEnumerable<OptionsModel>> DoThing(/*string term*/ IEnumerable<string> queries, CancellationToken token)
        {
            //  var queries = term.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var items = queries.Select(q => new OptionsModel()
            {
                Code = Guid.NewGuid().ToString(),
                Name = q
            })
                .GroupBy(x => x.Name)
                .Select(group => group.First())
                .ToList();

            return await Task.FromResult(new ReadOnlyCollection<OptionsModel>(items));
        }
    }
}
