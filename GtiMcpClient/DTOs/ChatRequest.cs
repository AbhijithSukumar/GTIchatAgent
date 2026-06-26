namespace GtiMcpClient.DTOs
{
    public record ChatRequest(
    string Prompt,
    string UserId,
    string Username
);
}
