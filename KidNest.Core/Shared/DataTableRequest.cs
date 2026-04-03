using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KidNest.Core.Shared
{
    public class DataTableRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public List<DataTableOrder> Order { get; set; } = new();
        public List<DataTableColumn> Columns { get; set; } = new();
        public Search Search { get; set; } = new();
    }

    public class DataTableOrder
    {
        public int Column { get; set; }
        public string Dir { get; set; } = "asc";
    }

    public class DataTableColumn
    {
        public string Data { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool Searchable { get; set; }
        public bool Orderable { get; set; }
        public Search Search { get; set; } = new();
    }

    public class Search
    {
        public string Value { get; set; } = string.Empty;
        public bool Regex { get; set; }
    }
}
