using MongoDB.Bson.Serialization.Attributes;

namespace StoreService.Model.Entities.Address;

public class UserAddress : ModelMongoDB
{
    /// <summary>
    /// UserId
    /// </summary>
    [BsonElement("userId")]
    public string UserId;
    /// <summary>
    /// AddressId
    /// </summary>
    [BsonElement("addressId")]
    public string AddressId;
}
