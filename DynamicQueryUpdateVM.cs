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

        public static IObservable<IList<OptionsModel>> FlattenChangeList(this IObservable<IChangeSet<IList<OptionsModel>>> source)
        {
            return source.Scan((List<OptionsModel>)null, (cache, changes) =>
            {
                if (cache == null)
                    cache = new List<OptionsModel>(changes.Count);

                foreach (var change in changes)
                {
                    switch (change.Reason)
                    {
                        case ListChangeReason.Add:
                            {
                                cache.AddRange(change.Item.Current, change.Item.CurrentIndex);
                            }
                            break;
                        case ListChangeReason.AddRange:
                            {
                                cache.AddRange(change.Range.SelectMany(x => x), change.Range.Index);
                            }
                            break;
                        //case ListChangeReason.Remove:
                        //    {
                        //        cache.RemoveMany(change.Item.Current);
                        //    }
                        //    break;
                        //case ListChangeReason.RemoveRange:
                        //    {
                        //        cache.RemoveMany(change.Range.SelectMany(x => x));
                        //    }
                        //    break;
                    }
                }
                return cache;
            });
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
                .TransformAsync(DoThing)
                //.ToCollection()
                //.SelectMany(DoThing)
                .FlattenChangeList()
                .ObserveOnDispatcher()
                .Select(x => (IEnumerable<OptionsModel>)x)
                .ToProperty(this, @this => @this.SearchResults);
        }

        private async Task<IList<OptionsModel>> DoThing(string query/*, CancellationToken token*/)
        {
            var items = new List<OptionsModel>()
            {
                new OptionsModel()
                {
                    Code = Guid.NewGuid().ToString(),
                    Name = query,
                }
            };

            return await Task.FromResult(new ReadOnlyCollection<OptionsModel>(items));
        }
    }
}
