using Dapper;
using DTO.Models.Master.AddressMaster;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Repository.DAL.Interface.Master.AddressMaster;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.DAL.Imple.Master.AddressMaster
{
    public class AddressMasterDAL : IAddressMasterDAL
    {
        private readonly string connq;

        public AddressMasterDAL(IConfiguration configuration)
        {
            connq = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        #region tblAddressTO
        public async Task<List<tblAddressTO>> GetAllAddressesDAL()
        {
            using var conn = new SqlConnection(connq);
            string sql = "SELECT * FROM tblAddress WHERE IsActive = 1";
            var result = await conn.QueryAsync<tblAddressTO>(sql);
            return result.ToList();
        }

        public async Task<tblAddressTO> GetAddressByIdDAL(int idAddress)
        {
            using var conn = new SqlConnection(connq);
            string sql = "SELECT * FROM tblAddress WHERE IdAddress = @IdAddress";
            var result = await conn.QueryFirstOrDefaultAsync<tblAddressTO>(sql, new { IdAddress = idAddress });
            return result;
        }
        public async Task<tblAddressResponseTO> GetAddressByIdUser(int idUser)
        {
            using var conn = new SqlConnection(connq);
            string sql = @"SELECT tblAddressMapping.IdAddressMapping,tblAddress.*,
          dimCountry.CountryName,dimCountry.CountryCode,dimState.StateName,dimState.StateCode,dimCity.CityName,dimCity.CityCode
            FROM tblAddressMapping tblAddressMapping 
            LEFT JOIN tblAddress ON tblAddressMapping.IdAddress = tblAddress.IdAddress
            LEFT JOIN dimCountry dimCountry ON dimCountry.IdCountry = tblAddress.IdCountry
            LEFT JOIN dimState dimState ON dimState.IdState = tblAddress.IdState
            LEFT JOIN dimCity dimCity ON dimCity.IdCity = tblAddress.IdCity
                  WHERE tblAddressMapping.EntityId = 30
";
            var result = await conn.QueryFirstOrDefaultAsync<tblAddressResponseTO>(sql, new { idUser = idUser });
            return result;
        }

        public async Task<int> AddAddressDAL(tblAddressTO address)
        {
            using var conn = new SqlConnection(connq);
            var parameter = new DynamicParameters();
            parameter.Add("@AddressLine1", address.AddressLine1, DbType.String);
            parameter.Add("@AddressLine2", address.AddressLine2, DbType.String);
            parameter.Add("@Landmark", address.Landmark, DbType.String);
            parameter.Add("@Area", address.Area, DbType.String);
            parameter.Add("@Locality", address.Locality, DbType.String);
            parameter.Add("@IdCity", address.IdCity, DbType.Int32);
            parameter.Add("@IdState", address.IdState, DbType.Int32);
            parameter.Add("@IdCountry", address.IdCountry, DbType.Int32);
            parameter.Add("@Pincode", address.Pincode, DbType.String);
            parameter.Add("@Latitude", address.Latitude, DbType.Decimal);
            parameter.Add("@Longitude", address.Longitude, DbType.Decimal);
            parameter.Add("@IsActive", address.IsActive, DbType.Boolean);
            parameter.Add("@CreatedOn", DateTime.Now, DbType.DateTime);
            parameter.Add("@CreatedBy", address.CreatedBy, DbType.Int32);

            string sql = @"INSERT INTO tblAddress 
                           (AddressLine1, AddressLine2, Landmark, Area, Locality, IdCity, IdState, IdCountry, Pincode, Latitude, Longitude, IsActive, CreatedOn, CreatedBy) 
                           VALUES 
                           (@AddressLine1, @AddressLine2, @Landmark, @Area, @Locality, @IdCity, @IdState, @IdCountry, @Pincode, @Latitude, @Longitude, @IsActive, @CreatedOn, @CreatedBy);
                           SELECT CAST(SCOPE_IDENTITY() as int);";

            return await conn.ExecuteScalarAsync<int>(sql, parameter);
        }

        public async Task<int> UpdateAddressDAL(tblAddressTO address)
        {
            using var conn = new SqlConnection(connq);
            var parameter = new DynamicParameters();
            parameter.Add("@IdAddress", address.IdAddress, DbType.Int32);
            parameter.Add("@AddressLine1", address.AddressLine1, DbType.String);
            parameter.Add("@AddressLine2", address.AddressLine2, DbType.String);
            parameter.Add("@Landmark", address.Landmark, DbType.String);
            parameter.Add("@Area", address.Area, DbType.String);
            parameter.Add("@Locality", address.Locality, DbType.String);
            parameter.Add("@IdCity", address.IdCity, DbType.Int32);
            parameter.Add("@IdState", address.IdState, DbType.Int32);
            parameter.Add("@IdCountry", address.IdCountry, DbType.Int32);
            parameter.Add("@Pincode", address.Pincode, DbType.String);
            parameter.Add("@Latitude", address.Latitude, DbType.Decimal);
            parameter.Add("@Longitude", address.Longitude, DbType.Decimal);
            parameter.Add("@IsActive", address.IsActive, DbType.Boolean);
            parameter.Add("@UpdatedOn", DateTime.Now, DbType.DateTime);
            parameter.Add("@UpdatedBy", address.UpdatedBy, DbType.Int32);

            string sql = @"UPDATE tblAddress SET 
                           AddressLine1 = @AddressLine1, 
                           AddressLine2 = @AddressLine2, 
                           Landmark = @Landmark, 
                           Area = @Area, 
                           Locality = @Locality, 
                           IdCity = @IdCity, 
                           IdState = @IdState, 
                           IdCountry = @IdCountry, 
                           Pincode = @Pincode, 
                           Latitude = @Latitude, 
                           Longitude = @Longitude, 
                           IsActive = @IsActive, 
                           UpdatedOn = @UpdatedOn, 
                           UpdatedBy = @UpdatedBy 
                           WHERE IdAddress = @IdAddress";

            return await conn.ExecuteAsync(sql, parameter);
        }

        public async Task<int> DeleteAddressDAL(int idAddress)
        {
            using var conn = new SqlConnection(connq);
            string sql = "DELETE FROM tblAddress WHERE IdAddress = @IdAddress";
            return await conn.ExecuteAsync(sql, new { IdAddress = idAddress });
        }
        #endregion

        #region dimAddressTypeTO
        public async Task<List<dimAddressTypeTO>> GetAllAddressTypesDAL()
        {
            using var conn = new SqlConnection(connq);
            string sql = "SELECT * FROM dimAddressType WHERE IsActive = 1";
            var result = await conn.QueryAsync<dimAddressTypeTO>(sql);
            return result.ToList();
        }

        public async Task<dimAddressTypeTO> GetAddressTypeByIdDAL(int idAddressType)
        {
            using var conn = new SqlConnection(connq);
            string sql = "SELECT * FROM dimAddressType WHERE IdAddressType = @IdAddressType";
            return await conn.QueryFirstOrDefaultAsync<dimAddressTypeTO>(sql, new { IdAddressType = idAddressType });
        }

        public async Task<int> AddAddressTypeDAL(dimAddressTypeTO addressType)
        {
            using var conn = new SqlConnection(connq);
            var parameter = new DynamicParameters();
            parameter.Add("@AddressTypeName", addressType.AddressTypeName, DbType.String);
            parameter.Add("@Description", addressType.Description, DbType.String);
            parameter.Add("@IsActive", addressType.IsActive, DbType.Boolean);
            parameter.Add("@CreatedOn", DateTime.Now, DbType.DateTime);
            parameter.Add("@CreatedBy", addressType.CreatedBy, DbType.Int32);

            string sql = @"INSERT INTO dimAddressType 
                           (AddressTypeName, Description, IsActive, CreatedOn, CreatedBy) 
                           VALUES 
                           (@AddressTypeName, @Description, @IsActive, @CreatedOn, @CreatedBy);
                           SELECT CAST(SCOPE_IDENTITY() as int);";

            return await conn.ExecuteScalarAsync<int>(sql, parameter);
        }

        public async Task<int> UpdateAddressTypeDAL(dimAddressTypeTO addressType)
        {
            using var conn = new SqlConnection(connq);
            var parameter = new DynamicParameters();
            parameter.Add("@IdAddressType", addressType.IdAddressType, DbType.Int32);
            parameter.Add("@AddressTypeName", addressType.AddressTypeName, DbType.String);
            parameter.Add("@Description", addressType.Description, DbType.String);
            parameter.Add("@IsActive", addressType.IsActive, DbType.Boolean);
            parameter.Add("@UpdatedOn", DateTime.Now, DbType.DateTime);
            parameter.Add("@UpdatedBy", addressType.UpdatedBy, DbType.Int32);

            string sql = @"UPDATE dimAddressType SET 
                           AddressTypeName = @AddressTypeName, 
                           Description = @Description, 
                           IsActive = @IsActive, 
                           UpdatedOn = @UpdatedOn, 
                           UpdatedBy = @UpdatedBy 
                           WHERE IdAddressType = @IdAddressType";

            return await conn.ExecuteAsync(sql, parameter);
        }

        public async Task<int> DeleteAddressTypeDAL(int idAddressType)
        {
            using var conn = new SqlConnection(connq);
            string sql = "DELETE FROM dimAddressType WHERE IdAddressType = @IdAddressType";
            return await conn.ExecuteAsync(sql, new { IdAddressType = idAddressType });
        }
        #endregion

        #region dimCityTO
        public async Task<List<dimCityTO>> GetAllCitiesDAL()
        {
            using var conn = new SqlConnection(connq);
            string sql = "SELECT * FROM dimCity WHERE IsActive = 1";
            var result = await conn.QueryAsync<dimCityTO>(sql);
            return result.ToList();
        }

        public async Task<dimCityTO> GetCityByIdDAL(int idCity)
        {
            using var conn = new SqlConnection(connq);
            string sql = "SELECT * FROM dimCity WHERE IdCity = @IdCity";
            return await conn.QueryFirstOrDefaultAsync<dimCityTO>(sql, new { IdCity = idCity });
        }

        public async Task<int> AddCityDAL(dimCityTO city)
        {
            using var conn = new SqlConnection(connq);
            var parameter = new DynamicParameters();
            parameter.Add("@IdState", city.IdState, DbType.Int32);
            parameter.Add("@CityCode", city.CityCode, DbType.String);
            parameter.Add("@CityName", city.CityName, DbType.String);
            parameter.Add("@IsActive", city.IsActive, DbType.Boolean);
            parameter.Add("@CreatedOn", DateTime.Now, DbType.DateTime);
            parameter.Add("@CreatedBy", city.CreatedBy, DbType.Int32);

            string sql = @"INSERT INTO dimCity
                           (IdState, CityCode, CityName, IsActive, CreatedOn, CreatedBy) 
                           VALUES 
                           (@IdState, @CityCode, @CityName, @IsActive, @CreatedOn, @CreatedBy);
                           SELECT CAST(SCOPE_IDENTITY() as int);";

            return await conn.ExecuteScalarAsync<int>(sql, parameter);
        }

        public async Task<int> UpdateCityDAL(dimCityTO city)
        {
            using var conn = new SqlConnection(connq);
            var parameter = new DynamicParameters();
            parameter.Add("@IdCity", city.IdCity, DbType.Int32);
            parameter.Add("@IdState", city.IdState, DbType.Int32);
            parameter.Add("@CityCode", city.CityCode, DbType.String);
            parameter.Add("@CityName", city.CityName, DbType.String);
            parameter.Add("@IsActive", city.IsActive, DbType.Boolean);
            parameter.Add("@UpdatedOn", DateTime.Now, DbType.DateTime);
            parameter.Add("@UpdatedBy", city.UpdatedBy, DbType.Int32);

            string sql = @"UPDATE dimCity SET 
                           IdState = @IdState, 
                           CityCode = @CityCode, 
                           CityName = @CityName, 
                           IsActive = @IsActive, 
                           UpdatedOn = @UpdatedOn, 
                           UpdatedBy = @UpdatedBy 
                           WHERE IdCity = @IdCity";

            return await conn.ExecuteAsync(sql, parameter);
        }

        public async Task<int> DeleteCityDAL(int idCity)
        {
            using var conn = new SqlConnection(connq);
            string sql = "DELETE FROM dimCity WHERE IdCity = @IdCity";
            return await conn.ExecuteAsync(sql, new { IdCity = idCity });
        }
        #endregion

        #region dimCountryTO
        public async Task<List<dimCountryTO>> GetAllCountriesDAL()
        {
            using var conn = new SqlConnection(connq);
            string sql = "SELECT * FROM dimCountry WHERE IsActive = 1";
            var result = await conn.QueryAsync<dimCountryTO>(sql);
            return result.ToList();
        }

        public async Task<dimCountryTO> GetCountryByIdDAL(int idCountry)
        {
            using var conn = new SqlConnection(connq);
            string sql = "SELECT * FROM dimCountry WHERE IdCountry = @IdCountry";
            return await conn.QueryFirstOrDefaultAsync<dimCountryTO>(sql, new { IdCountry = idCountry });
        }

        public async Task<int> AddCountryDAL(dimCountryTO country)
        {
            using var conn = new SqlConnection(connq);
            var parameter = new DynamicParameters();
            parameter.Add("@CountryCode", country.CountryCode, DbType.String);
            parameter.Add("@CountryName", country.CountryName, DbType.String);
            parameter.Add("@IsActive", country.IsActive, DbType.Boolean);
            parameter.Add("@CreatedOn", DateTime.Now, DbType.DateTime);
            parameter.Add("@CreatedBy", country.CreatedBy, DbType.Int32);

            string sql = @"INSERT INTO dimCountry 
                           (CountryCode, CountryName, IsActive, CreatedOn, CreatedBy) 
                           VALUES 
                           (@CountryCode, @CountryName, @IsActive, @CreatedOn, @CreatedBy);
                           SELECT CAST(SCOPE_IDENTITY() as int);";

            return await conn.ExecuteScalarAsync<int>(sql, parameter);
        }

        public async Task<int> UpdateCountryDAL(dimCountryTO country)
        {
            using var conn = new SqlConnection(connq);
            var parameter = new DynamicParameters();
            parameter.Add("@IdCountry", country.IdCountry, DbType.Int32);
            parameter.Add("@CountryCode", country.CountryCode, DbType.String);
            parameter.Add("@CountryName", country.CountryName, DbType.String);
            parameter.Add("@IsActive", country.IsActive, DbType.Boolean);
            parameter.Add("@UpdatedOn", DateTime.Now, DbType.DateTime);
            parameter.Add("@UpdatedBy", country.UpdatedBy, DbType.Int32);

            string sql = @"UPDATE dimCountry SET 
                           CountryCode = @CountryCode, 
                           CountryName = @CountryName, 
                           IsActive = @IsActive, 
                           UpdatedOn = @UpdatedOn, 
                           UpdatedBy = @UpdatedBy 
                           WHERE IdCountry = @IdCountry";

            return await conn.ExecuteAsync(sql, parameter);
        }

        public async Task<int> DeleteCountryDAL(int idCountry)
        {
            using var conn = new SqlConnection(connq);
            string sql = "DELETE FROM dimCountry WHERE IdCountry = @IdCountry";
            return await conn.ExecuteAsync(sql, new { IdCountry = idCountry });
        }
        #endregion

        #region dimStateTO
        public async Task<List<dimStateTO>> GetAllStatesDAL()
        {
            using var conn = new SqlConnection(connq);
            string sql = "SELECT * FROM dimState WHERE IsActive = 1";
            var result = await conn.QueryAsync<dimStateTO>(sql);
            return result.ToList();
        }

        public async Task<dimStateTO> GetStateByIdDAL(int idState)
        {
            using var conn = new SqlConnection(connq);
            string sql = "SELECT * FROM dimState WHERE IdState = @IdState";
            return await conn.QueryFirstOrDefaultAsync<dimStateTO>(sql, new { IdState = idState });
        }

        public async Task<int> AddStateDAL(dimStateTO state)
        {
            using var conn = new SqlConnection(connq);
            var parameter = new DynamicParameters();
            parameter.Add("@IdCountry", state.IdCountry, DbType.Int32);
            parameter.Add("@StateCode", state.StateCode, DbType.String);
            parameter.Add("@StateName", state.StateName, DbType.String);
            parameter.Add("@IsActive", state.IsActive, DbType.Boolean);
            parameter.Add("@CreatedOn", DateTime.Now, DbType.DateTime);
            parameter.Add("@CreatedBy", state.CreatedBy, DbType.Int32);

            string sql = @"INSERT INTO dimState 
                           (IdCountry, StateCode, StateName, IsActive, CreatedOn, CreatedBy) 
                           VALUES 
                           (@IdCountry, @StateCode, @StateName, @IsActive, @CreatedOn, @CreatedBy);
                           SELECT CAST(SCOPE_IDENTITY() as int);";

            return await conn.ExecuteScalarAsync<int>(sql, parameter);
        }

        public async Task<int> UpdateStateDAL(dimStateTO state)
        {
            using var conn = new SqlConnection(connq);
            var parameter = new DynamicParameters();
            parameter.Add("@IdState", state.IdState, DbType.Int32);
            parameter.Add("@IdCountry", state.IdCountry, DbType.Int32);
            parameter.Add("@StateCode", state.StateCode, DbType.String);
            parameter.Add("@StateName", state.StateName, DbType.String);
            parameter.Add("@IsActive", state.IsActive, DbType.Boolean);
            parameter.Add("@UpdatedOn", DateTime.Now, DbType.DateTime);
            parameter.Add("@UpdatedBy", state.UpdatedBy, DbType.Int32);

            string sql = @"UPDATE dimState SET 
                           IdCountry = @IdCountry, 
                           StateCode = @StateCode, 
                           StateName = @StateName, 
                           IsActive = @IsActive, 
                           UpdatedOn = @UpdatedOn, 
                           UpdatedBy = @UpdatedBy 
                           WHERE IdState = @IdState";

            return await conn.ExecuteAsync(sql, parameter);
        }

        public async Task<int> DeleteStateDAL(int idState)
        {
            using var conn = new SqlConnection(connq);
            string sql = "DELETE FROM dimState WHERE IdState = @IdState";
            return await conn.ExecuteAsync(sql, new { IdState = idState });
        }
        #endregion

        #region tblAddressMappingTO
        public async Task<List<tblAddressMappingTO>> GetAllAddressMappingsDAL()
        {
            using var conn = new SqlConnection(connq);
            string sql = "SELECT * FROM tblAddressMapping WHERE IsActive = 1";
            var result = await conn.QueryAsync<tblAddressMappingTO>(sql);
            return result.ToList();
        }

        public async Task<tblAddressMappingTO> GetAddressMappingByIdDAL(int idAddressMapping)
        {
            using var conn = new SqlConnection(connq);
            string sql = "SELECT * FROM tblAddressMapping WHERE IdAddressMapping = @IdAddressMapping";
            return await conn.QueryFirstOrDefaultAsync<tblAddressMappingTO>(sql, new { IdAddressMapping = idAddressMapping });
        }

        public async Task<int> AddAddressMappingDAL(tblAddressMappingTO addressMapping)
        {
            using var conn = new SqlConnection(connq);
            var parameter = new DynamicParameters();
            parameter.Add("@EntityType", addressMapping.EntityType, DbType.String);
            parameter.Add("@EntityId", addressMapping.EntityId, DbType.Int32);
            parameter.Add("@IdAddress", addressMapping.IdAddress, DbType.Int32);
            parameter.Add("@IdAddressType", addressMapping.IdAddressType, DbType.Int32);
            parameter.Add("@IsDefault", addressMapping.IsDefault, DbType.Boolean);
            parameter.Add("@IsActive", addressMapping.IsActive, DbType.Boolean);
            parameter.Add("@CreatedOn", DateTime.Now, DbType.DateTime);
            parameter.Add("@CreatedBy", addressMapping.CreatedBy, DbType.Int32);

            string sql = @"INSERT INTO tblAddressMapping
                           (EntityType, EntityId, IdAddress, IdAddressType, IsDefault, IsActive, CreatedOn, CreatedBy) 
                           VALUES 
                           (@EntityType, @EntityId, @IdAddress, @IdAddressType, @IsDefault, @IsActive, @CreatedOn, @CreatedBy);
                           SELECT CAST(SCOPE_IDENTITY() as int);";

            return await conn.ExecuteScalarAsync<int>(sql, parameter);
        }

        public async Task<int> UpdateAddressMappingDAL(tblAddressMappingTO addressMapping)
        {
            using var conn = new SqlConnection(connq);
            var parameter = new DynamicParameters();
            parameter.Add("@IdAddressMapping", addressMapping.IdAddressMapping, DbType.Int32);
            parameter.Add("@EntityType", addressMapping.EntityType, DbType.String);
            parameter.Add("@EntityId", addressMapping.EntityId, DbType.Int32);
            parameter.Add("@IdAddress", addressMapping.IdAddress, DbType.Int32);
            parameter.Add("@IdAddressType", addressMapping.IdAddressType, DbType.Int32);
            parameter.Add("@IsDefault", addressMapping.IsDefault, DbType.Boolean);
            parameter.Add("@IsActive", addressMapping.IsActive, DbType.Boolean);
            parameter.Add("@UpdatedOn", DateTime.Now, DbType.DateTime);
            parameter.Add("@UpdatedBy", addressMapping.UpdatedBy, DbType.Int32);

            string sql = @"UPDATE tblAddressMapping SET 
                           EntityType = @EntityType, 
                           EntityId = @EntityId, 
                           IdAddress = @IdAddress, 
                           IdAddressType = @IdAddressType, 
                           IsDefault = @IsDefault, 
                           IsActive = @IsActive, 
                           UpdatedOn = @UpdatedOn, 
                           UpdatedBy = @UpdatedBy 
                           WHERE IdAddressMapping = @IdAddressMapping";

            return await conn.ExecuteAsync(sql, parameter);
        }

        public async Task<int> DeleteAddressMappingDAL(int idAddressMapping)
        {
            using var conn = new SqlConnection(connq);
            string sql = "DELETE FROM tblAddressMapping WHERE IdAddressMapping = @IdAddressMapping";
            return await conn.ExecuteAsync(sql, new { IdAddressMapping = idAddressMapping });
        }
        #endregion
    }
}
