using Bookstore.Domain;
using Bookstore.Domain.Books;
using Bookstore.Domain.ReferenceData;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bookstore.Web.Areas.Admin.Models.Inventory
{
    public class InventoryIndexViewModel : PaginatedViewModel
    {
        public List<InventoryIndexListItemViewModel> Items { get; set; } = new List<InventoryIndexListItemViewModel>();
        public BookFilters Filters { get; set; } = new BookFilters();
        public IEnumerable<SelectListItem> Publishers { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> BookTypes { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Genres { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> BookConditions { get; set; } = new List<SelectListItem>();

        public InventoryIndexViewModel()
        {
        }

        public InventoryIndexViewModel(IPaginatedList<Book> books, IEnumerable<ReferenceDataItem> referenceDataItems)
        {
            foreach (var book in books)
            {
                Items.Add(new InventoryIndexListItemViewModel { Id = book.Id, Name = book.Name, Author = book.Author, BookType = book.BookType.Text, Condition = book.Condition.Text, Genre = book.Genre.Text, Publisher = book.Publisher.Text, UpdatedOn = book.UpdatedOn, Year = book.Year.GetValueOrDefault(), Price = book.Price, Quantity = book.Quantity });
            }

            PageIndex = books.PageIndex;
            PageSize = books.Count;
            PageCount = books.TotalPages;
            HasNextPage = books.HasNextPage;
            HasPreviousPage = books.HasPreviousPage;
            PaginationButtons = books.GetPageList(5).ToList();
            BookConditions = referenceDataItems.Where(x => x.DataType == ReferenceDataType.Condition).Select(x => new SelectListItem { Text = x.Text, Value = x.Id.ToString() });
            BookTypes = referenceDataItems.Where(x => x.DataType == ReferenceDataType.BookType).Select(x => new SelectListItem { Text = x.Text, Value = x.Id.ToString() });
            Genres = referenceDataItems.Where(x => x.DataType == ReferenceDataType.Genre).Select(x => new SelectListItem { Text = x.Text, Value = x.Id.ToString() });
            Publishers = referenceDataItems.Where(x => x.DataType == ReferenceDataType.Publisher).Select(x => new SelectListItem { Text = x.Text, Value = x.Id.ToString() });
        }
    }

    public class InventoryIndexListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Publisher { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string BookType { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
