using System;
using System.Threading.Tasks;
using Platformex;

namespace Demo
{
    public static class DocumentService
    {
        public static IDocument GetDocument(this IDomain domain, DocumentId id) 
            => domain.GetAggregate<IDocument>(id.ToString());
        public static async Task<IDocument> CreateDocument(this IDomain domain, DocumentId id, string newName)
        {
            var document = domain.GetDocument(id);
            var result = await document.Do(new CreateDocument(id, newName));
            if (!result.IsSuccess) throw new Exception(result.Error);
            return document;
        }
    }
}