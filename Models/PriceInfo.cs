
public class PriceInfo {
    public int Id {get;set;}
    public int ContactId {get;set;}
    public int ProductId {get;set;}
    public decimal Price {get;set;}
    public bool IsCollaboratorPrice {get;set;}
    public int CollaboratorId {get;set;}
    public int CategoryId {get;set;}
}