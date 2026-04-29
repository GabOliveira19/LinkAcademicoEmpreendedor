CSharp LinkAcademicoEmpreendedor\Services\DynamicFormService.cs
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkAcademicoEmpreendedor.Data;
using LinkAcademicoEmpreendedor.Models;
using Microsoft.EntityFrameworkCore;

namespace LinkAcademicoEmpreendedor.Services
{
    public class DynamicFormService : IDynamicFormService
    {
        private readonly ApplicationDbContext _context;

        public DynamicFormService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<FieldDefinition>> GetFieldsByAreaIdAsync(int areaId)
        {
            return await _context.FieldDefinitions
                .Where(f => f.AreaId == areaId)
                .OrderBy(f => f.Ordem)
                .ToListAsync();
        }
    }
}