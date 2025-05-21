public class ProductWithCategoryAndAllergens
{
    public int ProductID { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int PortionQuantity { get; set; }
    public string PortionUnit { get; set; }
    public int TotalQuantity { get; set; }
    public int CategoryID { get; set; }
    public string CategoryName { get; set; }
    public string Allergens { get; set; }
}

public class MenuWithProducts
{
    public int MenuID { get; set; }
    public string MenuName { get; set; }
    public string Description { get; set; }
    public int CategoryID { get; set; }
    public string CategoryName { get; set; }
    public int ProductID { get; set; }
    public string ProductName { get; set; }
    public decimal ProductPrice { get; set; }
    public int ProductQuantity { get; set; }
    public string ProductUnit { get; set; }
}

public class ProductLowStock
{
    public int ProductID { get; set; }
    public string Name { get; set; }
    public decimal TotalQuantity { get; set; }
    public string PortionUnit { get; set; }
}
