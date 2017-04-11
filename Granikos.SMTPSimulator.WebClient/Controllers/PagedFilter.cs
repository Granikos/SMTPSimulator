using System;

namespace Granikos.SMTPSimulator.WebClient.Controllers
{
    public class PagedFilter
    {
        private int _pageSize;
        private int _pageNumber;

        public int PageSize
        {
            get { return _pageSize; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");
                _pageSize = value;
            }
        }

        public int PageNumber
        {
            get { return _pageNumber; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");
                _pageNumber = value;
            }
        }
    }
}