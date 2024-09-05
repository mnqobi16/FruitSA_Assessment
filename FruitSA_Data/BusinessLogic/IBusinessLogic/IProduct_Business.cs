using FruitSA_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FruitSA_DataAccess.BusinessLogic.IBusinessLogic
{
    public interface IProduct_Business
    {
        public Task<ProductDTO> Create(ProductDTO objDTO);
        public Task<ProductDTO> Update(ProductDTO objDTO);
        public Task<int> Delete(int id);
        public Task<ProductDTO> Get(int id);
        public Task<IEnumerable<ProductDTO>> GetAll();
    }
}
