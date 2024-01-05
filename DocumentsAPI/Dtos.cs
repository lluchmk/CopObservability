﻿namespace DocumentsAPI;

public record CreateDocumentRequest(IEnumerable<string> Customers, IEnumerable<string>? Products, IEnumerable<string> Errors);

public record CreateDocumentMessage(IEnumerable<string> Customers, IEnumerable<string>? Products, IEnumerable<string> Errors, Guid documentId)
    : CreateDocumentRequest(Customers, Products, Errors);