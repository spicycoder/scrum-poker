namespace ScrumPoker.AppHost.IntegrationTests;

public static class IntegrationTestHelpers
{
    public static int GetRoomIdFromLocation(HttpResponseMessage response)
    {
        response.Headers.Location.ShouldNotBeNull();
        var path = response.Headers.Location.AbsolutePath;
        var idString = path.Split('/').Last();
        return int.Parse(idString);
    }
}
