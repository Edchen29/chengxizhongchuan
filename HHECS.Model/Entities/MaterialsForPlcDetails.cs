using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HHECS.Model.Entities
{
    [Table("materialsForPlcDetails")]
    public partial class MaterialsForPlcDetails
    {
        /// <summary>
        /// 原材料ID
        /// </summary>
        [Column("materialsId")]
        public int MaterialsId { get; set; }

        /// <summary>
        /// 切断序号
        /// </summary>
        [Column("SerialNumber")]
        public string SerialNumber { get; set; }

        /// <summary>
        /// 分段名
        /// </summary>
        [Column("SectionName")]
        public string SectionName { get; set; }

        /// <summary>
        /// 管名
        /// </summary>
        [Column("PipeName")]
        public string PipeName { get; set; }

        /// <summary>
        /// 切断长
        /// </summary>
        [Column("length")]
        public int Length { get; set; }

        /// <summary>
        /// 管端
        /// </summary>
        [Column("PipeEnd")]
        public string PipeEnd { get; set; }

        /// <summary>
        /// 下料工位
        /// </summary>
        [Column("AssemblyStation")]
        public int AssemblyStation { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [Column("status")]
        public int Status { get; set; }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("_Identify")]
        public int _Identify { get; set; }
    }
}
