using DTO.Models.Master.AddressMaster;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repository.DAL.Interface.Master.AddressMaster
{
    public interface IAddressMasterDAL
    {
        // tblAddressTO
        Task<List<tblAddressTO>> GetAllAddressesDAL();
        Task<tblAddressTO> GetAddressByIdDAL(int idAddress);
        Task<tblAddressResponseTO> GetAddressByIdUser(int idUser);
        Task<int> AddAddressDAL(tblAddressTO address);
        Task<int> UpdateAddressDAL(tblAddressTO address);
        Task<int> DeleteAddressDAL(int idAddress);

        // dimAddressTypeTO
        Task<List<dimAddressTypeTO>> GetAllAddressTypesDAL();
        Task<dimAddressTypeTO> GetAddressTypeByIdDAL(int idAddressType);
        Task<int> AddAddressTypeDAL(dimAddressTypeTO addressType);
        Task<int> UpdateAddressTypeDAL(dimAddressTypeTO addressType);
        Task<int> DeleteAddressTypeDAL(int idAddressType);

        // dimCityTO
        Task<List<dimCityTO>> GetAllCitiesDAL();
        Task<dimCityTO> GetCityByIdDAL(int idCity);
        Task<int> AddCityDAL(dimCityTO city);
        Task<int> UpdateCityDAL(dimCityTO city);
        Task<int> DeleteCityDAL(int idCity);

        // dimCountryTO
        Task<List<dimCountryTO>> GetAllCountriesDAL();
        Task<dimCountryTO> GetCountryByIdDAL(int idCountry);
        Task<int> AddCountryDAL(dimCountryTO country);
        Task<int> UpdateCountryDAL(dimCountryTO country);
        Task<int> DeleteCountryDAL(int idCountry);

        // dimStateTO
        Task<List<dimStateTO>> GetAllStatesDAL();
        Task<dimStateTO> GetStateByIdDAL(int idState);
        Task<int> AddStateDAL(dimStateTO state);
        Task<int> UpdateStateDAL(dimStateTO state);
        Task<int> DeleteStateDAL(int idState);

        // tblAddressMappingTO
        Task<List<tblAddressMappingTO>> GetAllAddressMappingsDAL();
        Task<tblAddressMappingTO> GetAddressMappingByIdDAL(int idAddressMapping);
        Task<int> AddAddressMappingDAL(tblAddressMappingTO addressMapping);
        Task<int> UpdateAddressMappingDAL(tblAddressMappingTO addressMapping);
        Task<int> DeleteAddressMappingDAL(int idAddressMapping);
    }
}
