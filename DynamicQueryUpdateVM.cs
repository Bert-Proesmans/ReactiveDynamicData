using DynamicData;
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
                .SelectMany(term => term.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                .ToObservableChangeSet()
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
