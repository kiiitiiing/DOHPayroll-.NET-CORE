using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DOHPayroll
{
    public class PaginatedList<T> : List<T>
    {
        public int _pageIndex { get; private set; }
        public int _totalPages { get; private set; }
        public int _pageSize { get; private set; }
        public int _itemCount { get; private set; }
        public string _action { get; private set; }

        public PaginatedList(List<T> items, string action, int count, int pageIndex, int pageSize, int itemCount)
        {
            _pageIndex = pageIndex;
            _totalPages = (int)Math.Ceiling(count / (double)pageSize);
            _pageSize = pageSize;
            _itemCount = itemCount;
            _action = action;
            this.AddRange(items);
        }

        public bool HasPreviousPage { get { return (_pageIndex > 1); } }

        public bool HasNextPage { get { return (_pageIndex < _totalPages); } }

        public static PaginatedList<T> CreateAsync(List<T> source, string action, int pageIndex, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            return new PaginatedList<T>(items, action, count, pageIndex, pageSize, source.Count());
        }
    }
}
