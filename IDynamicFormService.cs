CSharp LinkAcademicoEmpreendedor\Services\IDynamicFormService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using LinkAcademicoEmpreendedor.Models;

namespace LinkAcademicoEmpreendedor.Services
{
    public interface IDynamicFormService
    {
        Task<List<FieldDefinition>> GetFieldsByAreaIdAsync(int areaId);
    }
}