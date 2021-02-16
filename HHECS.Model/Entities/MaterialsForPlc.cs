using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace HHECS.Model.Entities
{
    [Table("materialsForPlc")]
    public partial class MaterialsForPlc
    {
        [Column("id")]
        public int id { get; set; }
        /// <summary>
        /// 原材料壁厚
        /// </summary>

        [Column("thickness")]
        public decimal? Thickness { get; set; }
        /// <summary>
        /// 原材料长度
        /// </summary>

        [Column("length")]
        public int Length { get; set; }

        /// <summary>
        /// 状态 :  未发送给ECS：0，已发送给ECS：1
        /// </summary>
        [Column("status")]
        public int Status { get; set; }
        /// <summary>
        /// 原材料类型
        /// </summary>

        [Column("materialType")]
        public string MaterialType { get; set; }

        /// <summary>
        /// 原材料直径
        /// </summary>
        [Column("diameter")]
        public decimal? Diameter { get; set; }
        /// <summary>
        /// 原材料ID
        /// </summary>

        [Column("materialsId")]
        public int materialsId { get; set; }

        [Column("套料编号")]
        public string TLCode { get; set; }

        [Column("套料日期")]
        public string TLDate { get; set; }
    }
}
