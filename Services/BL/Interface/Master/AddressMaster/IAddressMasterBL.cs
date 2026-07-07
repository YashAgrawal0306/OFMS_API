using DTO.Models.Master.AddressMaster;

namespace Services.BL.Interface.Master.AddressMaster
{
    public interface IAddressMasterBL
    {
        // tblAddressTO
        Task<List<tblAddressTO>> GetAllAddressesBL();
        Task<tblAddressTO> GetAddressByIdBL(int idAddress);
        Task<tblAddressResponseTO> GetAddressByIdUser(int idUser);
        Task<int> AddAddressBL(tblAddressTO address);
        Task<int> UpdateAddressBL(tblAddressTO address);
        Task<int> DeleteAddressBL(int idAddress);

        // dimAddressTypeTO
        Task<List<dimAddressTypeTO>> GetAllAddressTypesBL();
        Task<dimAddressTypeTO> GetAddressTypeByIdBL(int idAddressType);
        Task<int> AddAddressTypeBL(dimAddressTypeTO addressType);
        Task<int> UpdateAddressTypeBL(dimAddressTypeTO addressType);
        Task<int> DeleteAddressTypeBL(int idAddressType);

        // dimCityTO
        Task<List<dimCityTO>> GetAllCitiesBL();
        Task<dimCityTO> GetCityByIdBL(int idCity);
        Task<int> AddCityBL(dimCityTO city);
        Task<int> UpdateCityBL(dimCityTO city);
        Task<int> DeleteCityBL(int idCity);

        // dimCountryTO
        Task<List<dimCountryTO>> GetAllCountriesBL();
        Task<dimCountryTO> GetCountryByIdBL(int idCountry);
        Task<int> AddCountryBL(dimCountryTO country);
        Task<int> UpdateCountryBL(dimCountryTO country);
        Task<int> DeleteCountryBL(int idCountry);

        // dimStateTO
        Task<List<dimStateTO>> GetAllStatesBL();
        Task<dimStateTO> GetStateByIdBL(int idState);
        Task<int> AddStateBL(dimStateTO state);
        Task<int> UpdateStateBL(dimStateTO state);
        Task<int> DeleteStateBL(int idState);

        // tblAddressMappingTO
        Task<List<tblAddressMappingTO>> GetAllAddressMappingsBL();
        Task<tblAddressMappingTO> GetAddressMappingByIdBL(int idAddressMapping);
        Task<int> AddAddressMappingBL(tblAddressMappingTO addressMapping);
        Task<int> UpdateAddressMappingBL(tblAddressMappingTO addressMapping);
        Task<int> DeleteAddressMappingBL(int idAddressMapping);
    }
}
