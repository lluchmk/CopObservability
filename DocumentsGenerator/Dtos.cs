namespace DocumentsGenerator;

public record Order(Guid Id, Guid ProductId, Guid CustomerId, int Amount);

public record Product(Guid Id, string Name, string Description);

public record Customer(Guid Id, string Name);

public record CreateDocumentMessage(IEnumerable<string> Customers, IEnumerable<string>? Products, IEnumerable<string> Errors, Guid documentId);
