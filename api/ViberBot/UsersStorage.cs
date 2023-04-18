namespace api.ViberBot;
public interface IUsersStorage
{
    bool TryAddImei(string userId, string imei);
    void SetImei(string userId, string iemi);
    bool Contains(string userId);
    bool TryGetImei(string userId, out string imei);
    void SetStage(string userId, ChatUIState stage);
    bool UserInAnyStage(string userId);
    ChatUIState GetState(string userId);
}

public class UsersStorage: IUsersStorage
{
    private readonly Dictionary<string, string> _usersImeis;
    private readonly Dictionary<string, ChatUIState> _usersNavigation;

    public UsersStorage()
    {
        _usersImeis = new();
        _usersNavigation = new();
    }

    public bool TryAddImei(string userId, string imei)
    {
        return _usersImeis.TryAdd(userId, imei);
    }

    public void SetImei(string userId, string iemi)
    {
        if (!_usersImeis.ContainsKey(userId))
        {
            _usersImeis.Add(userId, iemi);
            return;
        }

        _usersImeis[userId] = iemi;
    }

    public bool Contains(string userId)
    {
        return _usersImeis.ContainsKey(userId);
    }

    public bool TryGetImei(string userId, out string imei)
    {
        return _usersImeis.TryGetValue(userId, out imei);
    }

    public void SetStage(string userId, ChatUIState stage)
    {
        if (!_usersNavigation.ContainsKey(userId))
        {
            _usersNavigation.Add(userId, stage);
            return;
        }

        _usersNavigation[userId] = stage;
    }

    public bool UserInAnyStage(string userId)
    {
        return _usersNavigation.ContainsKey(userId);
    }

    public ChatUIState GetState(string userId)
    {
        return _usersNavigation[userId];
    }
}
