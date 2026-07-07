using DTO.Models.Master.AddressMaster;
using Repository.DAL.Interface.Master.AddressMaster;
using Services.BL.Interface.Master.AddressMaster;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.BL.Imple.Master.AddressMaster
{
    public class AddressMasterBL : IAddressMasterBL
    {
        private readonly IAddressMasterDAL _dal;

        public AddressMasterBL(IAddressMasterDAL dal)
        {
            _dal = dal;
        }

        #region tblAddressTO
        public async Task<List<tblAddressTO>> GetAllAddressesBL()
        {
            return await _dal.GetAllAddressesDAL();
        }

        public async Task<tblAddressTO> GetAddressByIdBL(int idAddress)
        {
            return await _dal.GetAddressByIdDAL(idAddress);
        }
        public async Task<tblAddressResponseTO> GetAddressByIdUser(int idUser)
        {
            return await _dal.GetAddressByIdUser(idUser);
        }

        public async Task<int> AddAddressBL(tblAddressTO address)
        {
            return await _dal.AddAddressDAL(address);
        }

        public async Task<int> UpdateAddressBL(tblAddressTO address)
        {
            return await _dal.UpdateAddressDAL(address);
        }

        public async Task<int> DeleteAddressBL(int idAddress)
        {
            return await _dal.DeleteAddressDAL(idAddress);
        }
        #endregion

        #region dimAddressTypeTO
        public async Task<List<dimAddressTypeTO>> GetAllAddressTypesBL()
        {
            return await _dal.GetAllAddressTypesDAL();
        }

        public async Task<dimAddressTypeTO> GetAddressTypeByIdBL(int idAddressType)
        {
            return await _dal.GetAddressTypeByIdDAL(idAddressType);
        }

        public async Task<int> AddAddressTypeBL(dimAddressTypeTO addressType)
        {
            return await _dal.AddAddressTypeDAL(addressType);
        }

        public async Task<int> UpdateAddressTypeBL(dimAddressTypeTO addressType)
        {
            return await _dal.UpdateAddressTypeDAL(addressType);
        }

        public async Task<int> DeleteAddressTypeBL(int idAddressType)
        {
            return await _dal.DeleteAddressTypeDAL(idAddressType);
        }
        #endregion

        #region dimCityTO
        public async Task<List<dimCityTO>> GetAllCitiesBL()
        {
            return await _dal.GetAllCitiesDAL();
        }

        public async Task<dimCityTO> GetCityByIdBL(int idCity)
        {
            return await _dal.GetCityByIdDAL(idCity);
        }

        public async Task<int> AddCityBL(dimCityTO city)
        {
            return await _dal.AddCityDAL(city);
        }

        public async Task<int> UpdateCityBL(dimCityTO city)
        {
            return await _dal.UpdateCityDAL(city);
        }

        public async Task<int> DeleteCityBL(int idCity)
        {
            return await _dal.DeleteCityDAL(idCity);
        }
        #endregion

        #region dimCountryTO
        public async Task<List<dimCountryTO>> GetAllCountriesBL()
        {
            return await _dal.GetAllCountriesDAL();
        }

        public async Task<dimCountryTO> GetCountryByIdBL(int idCountry)
        {
            return await _dal.GetCountryByIdDAL(idCountry);
        }

        public async Task<int> AddCountryBL(dimCountryTO country)
        {
            return await _dal.AddCountryDAL(country);
        }

        public async Task<int> UpdateCountryBL(dimCountryTO country)
        {
            return await _dal.UpdateCountryDAL(country);
        }

        public async Task<int> DeleteCountryBL(int idCountry)
        {
            return await _dal.DeleteCountryDAL(idCountry);
        }
        #endregion

        #region dimStateTO
        public async Task<List<dimStateTO>> GetAllStatesBL()
        {
            return await _dal.GetAllStatesDAL();
        }

        public async Task<dimStateTO> GetStateByIdBL(int idState)
        {
            return await _dal.GetStateByIdDAL(idState);
        }

        public async Task<int> AddStateBL(dimStateTO state)
        {
            return await _dal.AddStateDAL(state);
        }

        public async Task<int> UpdateStateBL(dimStateTO state)
        {
            return await _dal.UpdateStateDAL(state);
        }

        public async Task<int> DeleteStateBL(int idState)
        {
            return await _dal.DeleteStateDAL(idState);
        }
        #endregion

        #region tblAddressMappingTO
        public async Task<List<tblAddressMappingTO>> GetAllAddressMappingsBL()
        {
            return await _dal.GetAllAddressMappingsDAL();
        }

        public async Task<tblAddressMappingTO> GetAddressMappingByIdBL(int idAddressMapping)
        {
            return await _dal.GetAddressMappingByIdDAL(idAddressMapping);
        }

        public async Task<int> AddAddressMappingBL(tblAddressMappingTO addressMapping)
        {
            return await _dal.AddAddressMappingDAL(addressMapping);
        }

        public async Task<int> UpdateAddressMappingBL(tblAddressMappingTO addressMapping)
        {
            return await _dal.UpdateAddressMappingDAL(addressMapping);
        }

        public async Task<int> DeleteAddressMappingBL(int idAddressMapping)
        {
            return await _dal.DeleteAddressMappingDAL(idAddressMapping);
        }
        #endregion
    }
}
