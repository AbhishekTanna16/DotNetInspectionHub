using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Core.Entities;
public class InspectionFrequency
{
    public int InspectionFrequencyID { get; set; }
    public string FrequencyName { get; set; }

    public List<AssetInspection> AssetInspections { get; set; }
}
