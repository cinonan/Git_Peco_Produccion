namespace AzureSearch.Models.Publico.Models
{
    public class PaginationVariableModel
    {

        public static int ResultsPerPage
        {
            get
            {
                return 12;
            }
        }
        public static int MaxPageRange
        {
            get
            {
                return 10;
            }
        }

        public static int PageRangeDelta
        {
            get
            {
                return 2;
            }
        }
    }
}
