using ECommerce.Web.DTOs;

namespace ECommerce.Web.Helpers;

public static class GuestRecentlyViewedHelper
{
    private const string SessionKey = "GuestRecentlyViewed";
    private const int MaxItems = 20;

    public static void Add(ISession session, RecentlyViewedDto item)
    {
        var items = session.GetObject<List<RecentlyViewedDto>>(SessionKey)
                    ?? new List<RecentlyViewedDto>();

        items.RemoveAll(x => x.Id == item.Id);
        items.Insert(0, item);

        if (items.Count > MaxItems)
            items = items.Take(MaxItems).ToList();

        session.SetObject(SessionKey, items);
    }

    public static List<RecentlyViewedDto> Get(ISession session)
    {
        return session.GetObject<List<RecentlyViewedDto>>(SessionKey)
               ?? new List<RecentlyViewedDto>();
    }

    public static void Clear(ISession session)
    {
        session.Remove(SessionKey);
    }
}