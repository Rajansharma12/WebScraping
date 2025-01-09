namespace MyFirstPROJECT.Models
{
    public class ProductProperties
    {
        public string Name { get; set; }
        public string[] Description { get; set; }
        public string OfferPrice { get; set; }
        public string Brand { get; set; }
        public string[] Category {  get; set; }
        public string[] Images { get; set; }
        public string MRP { get; set; }
        public string ProductRating { get; set; }


        public Dictionary<string, string> TableData { get; set; }

    }
}
