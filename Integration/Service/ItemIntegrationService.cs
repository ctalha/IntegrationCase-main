using Integration.Common;
using Integration.Backend;
using System.Text;
using System.Security.Cryptography;
using Integration.Service.LockManagement.Handler;

namespace Integration.Service;

public sealed class ItemIntegrationService
{
    //This is a dependency that is normally fulfilled externally.
    private ItemOperationBackend ItemIntegrationBackend { get; set; } = new();

    private readonly ILockHandler _lockHandler;

    public ItemIntegrationService(ILockHandler lockHandler)
    {
        _lockHandler = lockHandler;
    }


    // This is called externally and can be called multithreaded, in parallel.
    // More than one item with the same content should not be saved. However,
    // calling this with different contents at the same time is OK, and should
    // be allowed for performance reasons.
    public Result SaveItem(string itemContent)
    {
        string idempotentToken = GenerateIdempotentToken(itemContent);

        return _lockHandler.Execute(() =>
        {

            // Check the backend to see if the content is already saved.
            if (ItemIntegrationBackend.FindItemsWithContent(itemContent).Count != 0)
            {
                Console.WriteLine($"[LOG] Duplicate item detected: {itemContent}");
                return new Result(false, $"Duplicate item received with content {itemContent}.");
            }

            var item = ItemIntegrationBackend.SaveItem(itemContent);
            Console.WriteLine($"[LOG] Item with content {itemContent} saved with id {item.Id}");
            return new Result(true, $"Item with content {itemContent} saved with id {item.Id}");

        }, idempotentToken, 60);
    }

    // This method should be improved, the token can be changed for small differences such as space character
    private static string GenerateIdempotentToken(string itemContent)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(itemContent));
        StringBuilder builder = new();
        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }

        string token = builder.ToString();
        Console.WriteLine($"[LOG] Generated idempotent token for content: {itemContent}");
        return token;
    }

    public List<Item> GetAllItems()
    {
        return ItemIntegrationBackend.GetAllItems();
    }
}