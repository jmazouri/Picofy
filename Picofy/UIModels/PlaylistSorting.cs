using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Picofy.UIModels
{
    public class PlaylistSorting
    {
        public string ColumnName { get; set; }
        public ListSortDirection? SortDirection { get; set; }
    }
}
