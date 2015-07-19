using System;
using System.Collections.Generic;

namespace Solr.Client.Test
{
    public class TechProduct
    {
        public string Id { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public string Manu { get; set; }
        public IEnumerable<string> Category { get; set; }
        public IEnumerable<string> Features { get; set; }
        public string Includes { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Price { get; set; }
        public int? Popularity { get; set; }
        public bool? InStock { get; set; }
        public IEnumerable<string> Title { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string Comments { get; set; }
        public string Author { get; set; }
        public string Keywords { get; set; }
        public string ResourceName { get; set; }
        public string Url { get; set; }
        public IEnumerable<string> ContentType { get; set; }
        public DateTime? LastModified { get; set; }
        public IEnumerable<string> Links { get; set; }
        public string Source { get; set; }
        public IEnumerable<string> Content { get; set; }
        public string Payloads { get; set; }
        public DateTime? ManufactureDate { get; set; }
    }
}