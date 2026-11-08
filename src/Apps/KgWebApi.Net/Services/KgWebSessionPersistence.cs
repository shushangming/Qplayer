using KgWebApi.Net.Data;
using KgWebApi.Net.Data.Entities;
using KuGou.Net.Protocol.Session;

namespace KgWebApi.Net.Services;

public sealed class KgWebSessionPersistence(KgWebApiDbContext dbContext, IKgWebSessionContext sessionContext)
    : ISessionPersistence
{
    public KgSession? Load()
    {
        try
        {
            var entity = dbContext.Sessions.Find(GetSessionKey());
            return entity is null ? null : ToSession(entity);
        }
        catch
        {
            return null;
        }
    }

    public void Save(KgSession session)
    {
        try
        {
            var sessionKey = GetSessionKey();
            var entity = dbContext.Sessions.Find(sessionKey);

            if (entity is null)
            {
                entity = new KgSessionEntity { SessionKey = sessionKey };
                dbContext.Sessions.Add(entity);
            }

            ApplySession(entity, session);
            entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
            dbContext.SaveChanges();
        }
        catch
        {
        }
    }

    public void Clear()
    {
        try
        {
            var entity = dbContext.Sessions.Find(GetSessionKey());
            if (entity is null) return;

            dbContext.Sessions.Remove(entity);
            dbContext.SaveChanges();
        }
        catch
        {
        }
    }

    private static KgSession ToSession(KgSessionEntity entity)
    {
        return new KgSession
        {
            UserId = entity.UserId,
            Token = entity.Token,
            VipType = entity.VipType,
            VipToken = entity.VipToken,
            Dfid = entity.Dfid,
            Mid = entity.Mid,
            Uuid = entity.Uuid,
            InstallDev = entity.InstallDev,
            InstallMac = entity.InstallMac,
            InstallGuid = entity.InstallGuid,
            T1 = entity.T1 ?? string.Empty
        };
    }

    private static void ApplySession(KgSessionEntity entity, KgSession session)
    {
        entity.UserId = session.UserId;
        entity.Token = session.Token;
        entity.VipType = session.VipType;
        entity.VipToken = session.VipToken;
        entity.Dfid = session.Dfid;
        entity.Mid = session.Mid;
        entity.Uuid = session.Uuid;
        entity.InstallDev = session.InstallDev;
        entity.InstallMac = session.InstallMac;
        entity.InstallGuid = session.InstallGuid;
        entity.T1 = string.IsNullOrWhiteSpace(session.T1) ? null : session.T1;
    }

    private string GetSessionKey()
    {
        return string.IsNullOrWhiteSpace(sessionContext.SessionKey)
            ? KgWebSessionDefaults.FallbackSessionKey
            : sessionContext.SessionKey;
    }
}
