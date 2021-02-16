using HHECS.Bll;
using HHECS.Controls;
using HHECS.EquipmentExcute;
using HHECS.EquipmentExcute.Machine;
using HHECS.EquipmentExcute.Marking;
using HHECS.EquipmentExcute.PipeLine;
using HHECS.EquipmentExcute.Truss;
using HHECS.Model.BllModel;
using HHECS.Model.Common;
using HHECS.Model.Controls;
using HHECS.Model.Entities;
using HHECS.Model.Enums;
using HHECS.Model.Enums.Car;
using HHECS.Model.Enums.PipeLine;
using HHECS.Model.Enums.SRM;
using HHECS.Model.LEDHelper;
using HHECS.Model.LEDHelper.LEDComponent.DefaultLEDComponent;
using HHECS.Model.PLCHelper.Implement;
using HHECS.Model.PLCHelper.Interfaces;
using HHECS.Model.PLCHelper.PLCComponent.HslComponent;
using HHECS.OPC;
using HHECS.View.Win;
using HHECS.ViewModel;
using HslCommunication.Profinet.Siemens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HHECS.View
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class WinMain : BaseWindow
    {
        #region 属性

        /// <summary>
        /// OPC地址
        /// </summary>
        public string PLCIP { get; set; }

        public IPLC PLC { get; set; } = null;

        public OPCHelp CarPLC { get; set; }

        public HslModBusImplement ModBus { get; set; } = null;

        /// <summary>
        /// 时钟同步锁
        /// </summary>
        private object locker = new object();

        /// <summary>
        /// 子窗体
        /// </summary>
        public Dictionary<string, Window> ChildrenWin { get; set; } = new Dictionary<string, Window>();

        /// <summary>
        /// 主控时钟
        /// </summary>
        private System.Timers.Timer timer = new System.Timers.Timer();

        /// <summary>
        /// 结束时钟标志
        /// true 结束；false 执行；
        /// </summary>
        private bool stop = true;

        /// <summary>
        /// 设备
        /// </summary>
        public List<Equipment> Equipments { get; set; }

        /// <summary>
        /// 设备属性
        /// </summary>
        public List<EquipmentProp> EquipmentProps { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        public List<EquipmentType> EquipmentTypes { get; set; }

        /// <summary>
        /// 设备属性模板
        /// </summary>
        public List<EquipmentTypeTemplate> EquipmentTypeTemplates { get; set; }

        /// <summary>
        /// 工位列表
        /// </summary>
        public List<Station> Stations { get; set; }

        #region 注释

        ///// <summary>
        ///// 堆垛机处理类
        ///// </summary>
        //public List<SRMExcute> StockerExcutes { get; set; } = new List<SRMExcute>();

        ///// <summary>
        ///// 站台处理类
        ///// </summary>
        //public List<StationExcute> StationExcutes { get; set; } = new List<StationExcute>();

        ///// <summary>
        ///// 站台处理类
        ///// </summary>
        //public List<MachineToolExcute> MachineToolExcutes { get; set; } = new List<MachineToolExcute>();

        ///// <summary>
        ///// 机器人处理类
        ///// </summary>
        //public List<RobotExcute> RobotExcutes { get; set; } = new List<RobotExcute>();

        ///// <summary>
        ///// 绗架处理类
        ///// </summary>
        //public List<TrussExcute> Trusss { get; set; } = new List<TrussExcute>();

        ///// <summary>
        ///// 堆垛机监控
        ///// </summary>
        //public List<SRMMonitor> StockerMonitors { get; set; }

        ///// <summary>
        ///// 站台模型监控类
        ///// </summary>
        //public List<StationModel> StationModels { get; set; } = new List<StationModel>();

        ///// <summary>
        ///// 缓存所有库位
        ///// </summary>
        //public List<Location> Locations { get; set; }

        #endregion 注释

        /// <summary>
        /// 输送线和缓存基类
        /// </summary>
        public List<PipeLineExcute> PipeLineExcutes { get; set; } = new List<PipeLineExcute>();

        /// <summary>
        /// 机器处理基类
        /// </summary>
        public List<MachineExcute> MachineExcutes { get; set; } = new List<MachineExcute>();

        /// <summary>
        /// 穿梭车处理类
        /// </summary>
        public List<CarExcute> CarExcutes { get; set; } = new List<CarExcute>();

        /// <summary>
        /// 站台模型监控类
        /// </summary>
        public List<PipeLineModel> PipeLineModels { get; set; } = new List<PipeLineModel>();

        /// <summary>
        /// 日志监控
        /// </summary>
        public LogInfo LogInfo { get; set; }

        /// <summary>
        ///  是否开始回传
        /// </summary>
        public bool start;

        //Stopwatch stopwatch = new Stopwatch();

        #endregion 属性

        public WinMain()
        {
            InitializeComponent();
        }

        #region 初始化

        private static bool IsOpen = false;

        private void Init()
        {
            //时钟初始化
            //InitTimer();

            //初始化设备
            this.InitEquipments();

            //初始LED
            //InitLED();

            this.WindowState = WindowState.Maximized;
            this.txtStaus.Text = $"欢迎：{App.User.UserName}   {DateTime.Now.ToLocalTime()} 仓库：{App.WarehouseCode}";

            //日志初始化
            Logger.LogWrite += Logger_LogWrite;
            LogInfo = new LogInfo(550d, 600d);
            this.LogPanel.Children.Add(LogInfo);

            //1号小车信息显示
            CarInfo car1Info = new CarInfo() { ControlName = "Car1" };
            //car1Info.CommandCharge += CarInfo_CommandCharge;
            car1Info.CommandControlMode += CarInfo_CommandControlMode;
            car1Info.CommandDelete += CarInfo_CommandDelete;
            car1Info.CommandReset += CarInfo_CommandReset;
            car1Info.CommandSetGrap += CarInfo_CommandSetGrap;
            //car1Info.CommandFabricDistance += CarInfo_CommandFabricDistance;
            //car1Info.CommandFabricSpeed += CarInfo_CommandFabricSpeed;
            car1Info.CommandReSend += CarInfo_CommandReSend;
            car1Info.CommandUpdateLocation += CarInfo_CommandUpdateLocation;
            car1Info.CommandSetPosition += CarInfo_CommandSetPosition;
            this.CarPanel.Children.Add(car1Info);

            #region 初始化PLC连接参数

            try
            {
                //OPC使用
                //var opcConfigResult = AppSession.BllService.GetConfig(SysConst.OPCServerIP.ToString());
                //if (opcConfigResult.Success)
                //{
                //    PLCIP = opcConfigResult.Data.Value;
                //    PLC = new OPCImplement(PLCIP)
                //    {
                //        Equipments = Equipments
                //    };
                //}
                //else
                //{
                //    MessageBox.Show("未配置PLC，初始化失败");
                //}

                //纯S7实现
                //var plcDict = AppSession.BllService.GetDictWithDetails(SysConst.PLC.ToString());
                //if (!plcDict.Success || plcDict.Data.DictDetails == null || plcDict.Data.DictDetails.Count() == 0)
                //{
                //    MessageBox.Show("未配置PLC，初始化失败");
                //}
                //else
                //{
                //    List<S7PLCHelper> list = new List<S7PLCHelper>();
                //    foreach (var item in plcDict.Data.DictDetails)
                //    {
                //        list.Add(new S7PLCHelper()
                //        {
                //            PLCIP = item.Value,
                //            PLCType = item.Code
                //        });
                //    }
                //    PLC = new S7Implement()
                //    {
                //        S7PLCHelpers = list
                //    };
                //}

                //var plcDict = AppSession.BllService.GetDictWithDetails(SysConst.PLC.ToString());
                //List<SiemensPLCBuildModel> siemensPLCBuildModels = new List<SiemensPLCBuildModel>();
                //foreach (var item in plcDict.Data.DictDetails)
                //{
                //    siemensPLCBuildModels.Add(new SiemensPLCBuildModel()
                //    {
                //        SiemensPLCS = SiemensPLCS.S1500,
                //        IP = item.Value,
                //        Port = 102,
                //        Rack = 0,
                //        Slot = 0
                //    });
                //}
                //PLC = new HslSiemensImplement(siemensPLCBuildModels);

                List<SiemensPLCBuildModel> siemensPLCBuildModels = new List<SiemensPLCBuildModel>();
                foreach (var item in Equipments.Select(t => t.IP).Distinct())
                {
                    siemensPLCBuildModels.Add(new SiemensPLCBuildModel()
                    {
                        SiemensPLCS = SiemensPLCS.S1500,
                        IP = item,
                        Port = 102,
                        Rack = 0,
                        Slot = 0
                    });
                }
                PLC = new HslSiemensImplement(siemensPLCBuildModels);

                //var modBusDict = AppSession.BllService.GetDictWithDetails(SysConst.ModBus.ToString());
                //List<ModbusBuildModel> modbusBuildModel = new List<ModbusBuildModel>();
                //foreach (var item in modBusDict.Data.DictDetails)
                //{
                //    modbusBuildModel.Add(new ModbusBuildModel()
                //    {
                //        IP = item.Value,
                //        Port = 502,
                //        Station = 0x01,
                //    });
                //}
                //ModBus = new HslModBusImplement(modbusBuildModel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化PLC简介失败，请检查。{ex.Message}", "注意", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            #endregion 初始化PLC连接参数

            #region 开启处理调度线程

            CreateThread(() =>
            {
                EquipmentDispatch();
                //完毕后暂停，时间由配置文件定
                Thread.Sleep(1000);
            });

            #endregion 开启处理调度线程

            #region 监控界面按钮，如果循环停止了，就变按钮颜色。

            //CreateThread(() =>
            //{
            //    Thread.Sleep(100);
            //    ExcuteStatusInfo(stop);
            //});

            #endregion 监控界面按钮，如果循环停止了，就变按钮颜色。

            #region 回传RECS

            //CreateThread(() =>
            //{
            //    try
            //    {
            //        if (start == true)
            //        {
            //            WCSToRECS wCSToRECS = new WCSToRECS();
            //            wCSToRECS.WCSToRECSWA();
            //            if (Equipments != null)
            //            {
            //                wCSToRECS.WCSToRECSWS(Equipments);
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Logger.Log($"Socket数据回传出现异常：{ex.Message}", LogLevel.Error);
            //    }
            //    //完毕后暂停1000毫秒
            //    System.Threading.Thread.Sleep(1000);
            //});

            #endregion 回传RECS

            #region 同步PLC数据到数据库

            //CreateThread(async () =>
            //{
            //        await Task.Delay(1000);
            //        PlcToPc(stop);
            //})

            #endregion 同步PLC数据到数据库
        }

        /// <summary>
        /// 产生一个不停执行的背景线程
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private Thread CreateThread(Action action)
        {
            var thread = new Thread(() =>
            {
                while (true)
                {
                    action();
                }
            });
            thread.IsBackground = true;
            thread.Start();
            return thread;
        }

        private void InitLED()
        {
            var result = AppSession.Dal.GetCommonModelByCondition<LEDEntity>($"where warehouseCode ='{App.WarehouseCode}'");
            if (result.Success)
            {
                var leds = result.Data.Select(t => new LEDCreateOption()
                {
                    Code = t.Code,
                    Name = t.Name,
                    Remark = t.Remark,
                    IP = t.IP,
                    Port = t.Port,
                    Timesec = t.Timeout
                }).ToList();
                AppSession.LEDExcute = new DefaultLEDImplement(leds);
            }
            else
            {
                MessageBox.Show($"未能加载LED配置：{result.Msg}");
            }
        }

        private void InitEquipments()
        {
            try
            {
                // var result1 = AppSession.Dal.GetCommonModelByCondition<Equipment>($"where warehouseCode = '{App.WarehouseCode}' and enable = 0");
                var result1 = AppSession.Dal.GetCommonModelByCondition<Equipment>($"where enable = 1");
                var result2 = AppSession.Dal.GetCommonModelByCondition<EquipmentProp>("");
                var result3 = AppSession.Dal.GetCommonModelByCondition<EquipmentType>("");
                var result4 = AppSession.Dal.GetCommonModelByCondition<EquipmentTypeTemplate>("");
                var result5 = AppSession.Dal.GetCommonModelByCondition<Station>("");
                //var result6 = AppSession.Dal.GetCommonModelByCondition<StepStation>("");

                if (!result1.Success || !result2.Success || !result3.Success || !result4.Success || !result5.Success)
                {
                    MessageBox.Show("初始化设备信息异常");
                    Btn_BeginExcute.IsEnabled = false;
                    Btn_EndExcute.IsEnabled = false;
                    return;
                }

                Equipments = result1.Data;
                EquipmentProps = result2.Data.Where(t => Equipments.Count(a => a.Id == t.EquipmentId) > 0).ToList();
                EquipmentTypes = result3.Data.Where(t => Equipments.Count(a => a.EquipmentTypeId == t.Id) > 0).ToList();
                EquipmentTypeTemplates = result4.Data.Where(t => EquipmentTypes.Count(a => a.Id == t.EquipmentTypeId) > 0).ToList();
                Stations = result5.Data;
                //StepStations = result6.Data;

                //组合逻辑外键
                Equipments.ForEach(t =>
                {
                    t.EquipmentType = EquipmentTypes.FirstOrDefault(i => i.Id == t.EquipmentTypeId);
                    t.EquipmentProps.AddRange(EquipmentProps.Where(i => i.EquipmentId == t.Id).ToList());
                    //t.StepStationList = StepStations;
                    t.StationList = Stations;
                    t.Station = Stations.FirstOrDefault(i => i.Id == t.StationId);
                });
                EquipmentProps.ForEach(t =>
                {
                    t.Equipment = Equipments.FirstOrDefault(i => i.Id == t.EquipmentId);
                    //hack:组合地址，当未使用OPC时，请注释
                    //t.Address = $"S7:[{t.Equipment.ConnectName}]{t.Address}";

                    t.EquipmentTypeTemplate = EquipmentTypeTemplates.FirstOrDefault(i => i.Id == t.EquipmentTypeTemplateId);
                });
                //判断逻辑外键是否组合完毕
                if (Equipments.Count(t => t.EquipmentType == null || t.EquipmentProps.Count == 0) > 0)
                {
                    MessageBox.Show("初始化设备信息失败，请检查基础数据");
                    Btn_BeginExcute.IsEnabled = false;
                    Btn_EndExcute.IsEnabled = false;
                    return;
                }

                #region 组合监控类

                Equipments.ForEach(t =>
                {
                    if (t.EquipmentType.Code == "LengthMeasuringCache" || t.EquipmentType.Code == "BebelCache" || t.EquipmentType.Code == "AssembleConveyorLine" || t.EquipmentType.Code == "AssemblyCache")
                    {
                        PipeLineModels.Add(new PipeLineModel()
                        {
                            Code = t.Code,
                            Name = t.Name
                        });
                    }
                });

                Equipments.ForEach(t =>
                {
                    if (t.EquipmentType.Code == "MeasuringLength")
                    {
                        //测长监控
                        MeasuringLengthMonitorInfo measuringLengthMonitorInfo = new MeasuringLengthMonitorInfo(670, 500)
                        {
                            ControlName = t.Name,
                            Self = t
                        };
                        this.MeasuringLengthPanel.Children.Add(measuringLengthMonitorInfo);
                    }
                });

                Equipments.ForEach(t =>
                {
                    if (t.EquipmentType.Code == "Cutting")
                    {
                        //定长切割监控
                        CuttingMonitorInfo cuttingMonitorInfo = new CuttingMonitorInfo(750, 500)
                        {
                            ControlName = t.Name,
                            Self = t
                        };
                        this.CuttingMonitorInfoPanel.Children.Add(cuttingMonitorInfo);
                    }
                });

                Equipments.ForEach(t =>
                {
                    if (t.EquipmentType.Code == "Bevel")
                    {
                        //坡口机监控
                        BevelMonitorInfo bevelMonitorInfo = new BevelMonitorInfo(750, 500)
                        {
                            ControlName = t.Name,
                            Self = t
                        };
                        this.BevelMonitorPanel.Children.Add(bevelMonitorInfo);
                    }
                });

                Equipments.ForEach(t =>
                {
                    if (t.EquipmentType.Code == "Assemble")
                    {
                        //组对监控
                        AeesmblyMonitor AeesmblyMonitor = new AeesmblyMonitor(750, 500)
                        {
                            ControlName = t.Name,
                            Self = t
                        };
                        this.AeesmblyPanel.Children.Add(AeesmblyMonitor);
                    }
                });

                this.dgPipeLine.ItemsSource = PipeLineModels.OrderBy(t => t.Name).ToList();

                #endregion 组合监控类

                //组合处理类
                EquipmentTypes.ForEach(t =>
                {
                    //if (t.Code == "MeasuringLength")
                    //{
                    //    MachineExcutes.Add(new LengthMeasuringNormalExcute() { EquipmentType = t, Equipments = Equipments });
                    //}
                    //if (t.Code == "LengthMeasuringCache")
                    //{
                    //    PipeLineExcutes.Add(new LengthMeasuringCacheExcute() { EquipmentType = t, Equipments = Equipments });
                    //}
                    if (t.Code == "Cutting")
                    {
                        MachineExcutes.Add(new CutterNormalExcute() { EquipmentType = t, Equipments = Equipments });
                    }
                    if (t.Code == "BebelCache")
                    {
                        PipeLineExcutes.Add(new BevelCacheExcute() { EquipmentType = t, Equipments = Equipments });
                    }
                    //if (t.Code == "Bevel")
                    //{
                    //    MachineExcutes.Add(new BevelingNormalExcute() { EquipmentType = t, Equipments = Equipments });
                    //}
                    if (t.Code == "AssembleConveyorLine")
                    {
                        PipeLineExcutes.Add(new AssemblyConveyorLine() { EquipmentType = t, Equipments = Equipments });
                    }
                    if (t.Code == "AssemblyCache")
                    {
                        PipeLineExcutes.Add(new AssemblyCacheExcute() { EquipmentType = t, Equipments = Equipments });
                    }
                    //if (t.Code == "Assemble")
                    //{
                    //    MachineExcutes.Add(new AssemblyNormalExcute() { EquipmentType = t, Equipments = Equipments });
                    //}
                    if (t.Code == "RGV")
                    {
                        CarExcutes.Add(new CarNormalExcute() { EquipmentType = t, Equipments = Equipments });
                    }

                    #region 注释

                    //if (t.Code == "SingeForkSRM")
                    //{
                    //    StockerExcutes.Add(new SingeForkSRMExcute() { EquipmentType = t });
                    //}
                    //if (t.Code == "StationForStockerInOrOut")
                    //{
                    //    StationExcutes.Add(new StationForStockerInOrOutExcute() { EquipmentType = t, Equipments = Equipments });
                    //}
                    //if (t.Code == "StationForPortIn")
                    //{
                    //    StationExcutes.Add(new StationForPortInExcute() { EquipmentType = t, Equipments = Equipments });
                    //}
                    //if (t.Code == "StationForPortOut")
                    //{
                    //    StationExcutes.Add(new StationForPortOutExcute() { EquipmentType = t, Equipments = Equipments });
                    //}
                    //if (t.Code == "TransferStation")
                    //{
                    //    StationExcutes.Add(new TransferStationExcute() { EquipmentType = t, Equipments = Equipments });
                    //}
                    //if (t.Code == "Truss")
                    //{
                    //    Trusss.Add(new TrussNormalExcute() { EquipmentType = t });
                    //}
                    //if (t.Code == "WeldingType")
                    //{
                    //    RobotExcutes.Add(new RobotForWeld() { EquipmentType = t, Equipments = Equipments });
                    //}

                    #endregion 注释
                });

                #region 注释

                //Equipments.ForEach(t =>
                //{
                //    if (t.EquipmentType.Code.Contains("Station") && !t.EquipmentType.Code.Contains("StationForFinished"))
                //    {
                //        StationModels.Add(new StationModel()
                //        {
                //            Code = t.Code,
                //            Name = t.Name
                //        });
                //    }
                //});

                //Equipments.ForEach(t =>
                //{
                //    if (t.EquipmentType.Code == "Bevel")
                //    {
                //        //坡口机监控
                //        CacheMonitorInfo cacheMonitorInfo = new CacheMonitorInfo(750, 500)
                //        {
                //            ControlName = t.Name,
                //            Self = t
                //        };
                //        this.CachePanel.Children.Add(cacheMonitorInfo);
                //    }
                //});

                //this.DGStation.ItemsSource = StationModels.OrderBy(t => t.Name).ToList();

                #endregion 注释
            }
            catch (Exception ex)
            {
                MessageBox.Show("初始化设备信息出错：" + ex.Message, "注意", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion 初始化

        #region 小车事件处理

        /// <summary>
        /// 小车更新位置
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private BllResult CarInfo_CommandSetPosition(Equipment car)
        {
            try
            {
                if (CarPLC == null || CarPLC.GetConnStatus() == false)
                {
                    return BllResultFactory.Error(null, "地址读取失败,请检查通讯连接");
                }
                List<EquipmentProp> propsToWriter = new List<EquipmentProp>();
                var props = car.EquipmentProps;
                var wcsActionType = props.Find(t => t.EquipmentTypeTemplateCode == "wcsActionType");
                var wcsPosition = props.Find(t => t.EquipmentTypeTemplateCode == "wcsPosition");
                var wcsSwitch = props.Find(t => t.EquipmentTypeTemplateCode == "wcsSwitch");
                propsToWriter.AddRange(new List<EquipmentProp>() { wcsActionType, wcsPosition, wcsSwitch });
                var result = CarPLC.WriteAddress(propsToWriter);
                if (result.Success)
                {
                    Logger.Log($"{car.Code} 更新位置成功", LogLevel.Info);
                    return BllResultFactory.Sucess(null, "更新位置成功");
                }
                else
                {
                    Logger.Log($"{car.Code} 更新位置失败", LogLevel.Error);
                    return BllResultFactory.Error($"更新位置失败！");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"{car.Code} 更新位置失败", LogLevel.Error);
                return BllResultFactory.Error($"更新位置执行失败:{ex.Message}");
            }
        }

        /// <summary>
        ///  小车更新库位
        /// </summary>
        /// <param name="car"></param>
        /// <returns></returns>
        private BllResult CarInfo_CommandUpdateLocation(Equipment car)
        {
            try
            {
                if (CarPLC == null || CarPLC.GetConnStatus() == false)
                {
                    return BllResultFactory.Error(null, "地址读取失败,请检查通讯连接");
                }
                List<EquipmentProp> propsToWriter = new List<EquipmentProp>();
                var props = car.EquipmentProps;
                var wcsActionType = props.Find(t => t.EquipmentTypeTemplateCode == "wcsActionType");
                var wcsRow = props.Find(t => t.EquipmentTypeTemplateCode == "wcsRow");
                //var wcsLine = props.Find(t => t.EquipmentTypeTemplateCode == "wcsLine");
                //var wcsLayer = props.Find(t => t.EquipmentTypeTemplateCode == "wcsLayer");
                var wcsSwitch = props.Find(t => t.EquipmentTypeTemplateCode == "wcsSwitch");
                propsToWriter.AddRange(new List<EquipmentProp>() { wcsActionType, wcsRow, wcsSwitch });
                var result = CarPLC.WriteAddress(propsToWriter);
                if (result.Success)
                {
                    Logger.Log($"{car.Code} 更新库位成功", LogLevel.Info);
                    return BllResultFactory.Sucess(null, "更新库位成功");
                }
                else
                {
                    Logger.Log($"{car.Code} 更新库位失败", LogLevel.Error);
                    return BllResultFactory.Error($"更新库位失败！");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"{car.Code} 更新库位失败", LogLevel.Error);
                return BllResultFactory.Error($"更新库位执行失败:{ex.Message}");
            }
        }

        /// <summary>
        /// 小车重发任务
        /// </summary>
        /// <param name="car"></param>
        /// <returns></returns>
        private BllResult CarInfo_CommandReSend(Equipment car)
        {
            try
            {
                if (CarPLC == null || CarPLC.GetConnStatus() == false)
                {
                    //AddLogToUI("地址读取失败,请检查通讯连接", 2);
                    return BllResultFactory.Error(null, "地址读取失败,请检查通讯连接");
                }
                //#region 如果已经在充电桩上了，不能重新下发任务
                //string row = car.EquipmentProps.Find(t => t.EquipmentTypePropTemplateCode == "row").Value;
                //string line = car.EquipmentProps.Find(t => t.EquipmentTypePropTemplateCode == "line").Value;
                //string layer = car.EquipmentProps.Find(t => t.EquipmentTypePropTemplateCode == "layer").Value;

                //if (row == "3" && line == "23" && (layer == "1" || layer == "2"))
                //{
                //    return BllResultFactory.Error($"开始重下发失败，小车在充电桩上不能重下任务！");
                //}
                //#endregion
                string carNo = car.EquipmentProps.Find(t => t.EquipmentTypeTemplateCode == "carNo").Value;
                var TaskCarResult = AppSession.Dal.GetCommonModelByCondition<CarTask>($" where status < {TaskCarStatus.Executed.GetIndexInt()} and carNo={carNo}");
                if (TaskCarResult.Success)
                {
                    CarTask task = TaskCarResult.Data[0];
                    List<EquipmentProp> propsToWriter = new List<EquipmentProp>();
                    var props = car.EquipmentProps;
                    var wcsConfirmTaskFinish = props.Find(t => t.EquipmentTypeTemplateCode == "wcsConfirmTaskFinish");
                    wcsConfirmTaskFinish.Value = "0";
                    var wcsActionType = props.Find(t => t.EquipmentTypeTemplateCode == "wcsActionType");
                    wcsActionType.Value = task.Type.ToString();
                    //var wcsRow = props.Find(t => t.EquipmentTypeTemplateCode == "wcsRow");
                    //wcsRow.Value = task.Row.ToString();
                    //var wcsLine = props.Find(t => t.EquipmentTypeTemplateCode == "wcsLine");
                    //wcsLine.Value = task.Line.ToString();
                    //var wcsLayer = props.Find(t => t.EquipmentTypeTemplateCode == "wcsLayer");
                    //wcsLayer.Value = task.Layer.ToString();
                    var wcsTaskHeaderId = props.Find(t => t.EquipmentTypeTemplateCode == "wcsTaskHeaderId");
                    wcsTaskHeaderId.Value = task.StepTraceId.ToString();
                    var wcsTaskCarId = props.Find(t => t.EquipmentTypeTemplateCode == "wcsTaskCarId");
                    wcsTaskCarId.Value = task.Id.ToString();
                    var wcsSwitch = props.Find(t => t.EquipmentTypeTemplateCode == "wcsSwitch");
                    wcsSwitch.Value = "1";
                    propsToWriter.AddRange(new List<EquipmentProp>() { wcsConfirmTaskFinish, wcsActionType, wcsTaskHeaderId, wcsTaskCarId, wcsSwitch });
                    //return S7Helper.PlcSplitWrite(plc, propsToWriter, 20);
                    var result = CarPLC.WriteAddress(propsToWriter);
                    if (result.Success)
                    {
                        Logger.Log($"{car.Code} 重下下发成功", LogLevel.Info);
                        return BllResultFactory.Sucess(null, "重下下发成功");
                    }
                    else
                    {
                        //Logger.Log($"通知WMS取货错误处理成功，但是下发{stocker}删除任务{task.Id}失败！", LogLevel.Error);
                        Logger.Log($"{car.Code} 重下任务发失败", LogLevel.Error);
                        return BllResultFactory.Error($"重下任务发失败：写入PLC失败！");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"{car.Code} 重下下发成功", LogLevel.Error);
                return BllResultFactory.Error($"重下发失败：{ex.Message}");
            }
            Logger.Log($"{car.Code} 重下发任务失败，没有找到任务", LogLevel.Error);
            return BllResultFactory.Error("重下发任务失败，没有找到任务");
        }

        /// <summary>
        /// 小车设置间距
        /// </summary>
        /// <param name="car"></param>
        /// <returns></returns>
        private BllResult CarInfo_CommandSetGrap(Equipment car)
        {
            if (CarPLC == null || CarPLC.GetConnStatus() == false)
            {
                //AddLogToUI("地址读取失败,请检查通讯连接", 2);
                return BllResultFactory.Error(null, "地址读取失败,请检查通讯连接");
            }
            var wcsGrap = car.EquipmentProps.FirstOrDefault(t => t.EquipmentTypeTemplateCode == "wcsGrap");
            if (wcsGrap == null)
            {
                return BllResultFactory.Error(null, $"未找到小车{car.Name}的属性");
            }
            var result = CarPLC.WriteAddress(new List<EquipmentProp>() { wcsGrap });
            if (result.Success)
            {
                Logger.Log($"{car.Code} 开始设置间距成功", LogLevel.Info);
                return BllResultFactory.Sucess(null, "成功");
            }
            else
            {
                //Logger.Log($"通知WMS取货错误处理成功，但是下发{stocker}删除任务{task.Id}失败！", LogLevel.Error);
                Logger.Log($"{car.Code} 开始设置间距失败", LogLevel.Error);
                return BllResultFactory.Error($"开始设置间距失败！");
            }
        }

        /// <summary>
        /// 小车布料速度
        /// </summary>
        /// <param name="car"></param>
        /// <returns></returns>
        private BllResult CarInfo_CommandFabricSpeed(Equipment car)
        {
            if (CarPLC == null || CarPLC.GetConnStatus() == false)
            {
                //AddLogToUI("地址读取失败,请检查通讯连接", 2);
                return BllResultFactory.Error(null, "地址读取失败,请检查通讯连接");
            }
            var wcsFabricSpeed = car.EquipmentProps.FirstOrDefault(t => t.EquipmentTypeTemplateCode == "wcsFabricSpeed");
            if (wcsFabricSpeed == null)
            {
                return BllResultFactory.Error(null, $"未找到小车{car.Name}的属性");
            }
            var result = CarPLC.WriteAddress(new List<EquipmentProp>() { wcsFabricSpeed });
            if (result.Success)
            {
                Logger.Log($"{car.Code} 开始设置布料速度成功", LogLevel.Info);
                return BllResultFactory.Sucess(null, "成功");
            }
            else
            {
                Logger.Log($"{car.Code} 开始设置布料速度失败", LogLevel.Error);
                return BllResultFactory.Error($"开始设置布料速度失败！");
            }
        }

        /// <summary>
        /// 小车布料距离
        /// </summary>
        /// <param name="car"></param>
        /// <returns></returns>
        private BllResult CarInfo_CommandFabricDistance(Equipment car)
        {
            if (CarPLC == null || CarPLC.GetConnStatus() == false)
            {
                //AddLogToUI("地址读取失败,请检查通讯连接", 2);
                return BllResultFactory.Error(null, "地址读取失败,请检查通讯连接");
            }
            var wcsFabricDistance = car.EquipmentProps.FirstOrDefault(t => t.EquipmentTypeTemplateCode == "wcsFabricDistance");
            if (wcsFabricDistance == null)
            {
                return BllResultFactory.Error(null, $"未找到小车{car.Name}的属性");
            }
            var result = CarPLC.WriteAddress(new List<EquipmentProp>() { wcsFabricDistance });
            if (result.Success)
            {
                Logger.Log($"{car.Code} 开始设置布料距离成功", LogLevel.Info);
                return BllResultFactory.Sucess(null, "成功");
            }
            else
            {
                //Logger.Log($"通知WMS取货错误处理成功，但是下发{stocker}删除任务{task.Id}失败！", LogLevel.Error);
                Logger.Log($"{car.Code} 开始设置布料距离失败", LogLevel.Error);
                return BllResultFactory.Error($"开始设置布料距离失败！");
            }
        }

        /// <summary>
        /// 小车复位
        /// </summary>
        /// <param name="car"></param>
        /// <returns></returns>
        private BllResult CarInfo_CommandReset(Equipment car)
        {
            if (CarPLC == null || CarPLC.GetConnStatus() == false)
            {
                //AddLogToUI("地址读取失败,请检查通讯连接", 2);
                return BllResultFactory.Error(null, "地址读取失败,请检查通讯连接");
            }
            var taskProp = car.EquipmentProps.FirstOrDefault(t => t.EquipmentTypeTemplateCode == "wcsResetCommand");
            if (taskProp == null)
            {
                return BllResultFactory.Error(null, $"未找到小车{car.Name}的属性");
            }
            var result = CarPLC.WriteAddress(new List<EquipmentProp>() { taskProp });
            if (result.Success)
            {
                Logger.Log($"{car.Code} 复位成功", LogLevel.Info);
                return BllResultFactory.Sucess(null, "成功");
            }
            else
            {
                //Logger.Log($"通知WMS取货错误处理成功，但是下发{stocker}删除任务{task.Id}失败！", LogLevel.Error);
                Logger.Log($"{car.Code} 开始复位失败！", LogLevel.Error);
                return BllResultFactory.Error($"开始复位失败！");
            }
        }

        /// <summary>
        /// 小车删除任务
        /// </summary>
        /// <param name="car"></param>
        /// <returns></returns>
        private BllResult CarInfo_CommandDelete(Equipment car)
        {
            if (CarPLC == null || CarPLC.GetConnStatus() == false)
            {
                //AddLogToUI("地址读取失败,请检查通讯连接", 2);
                return BllResultFactory.Error(null, "地址读取失败,请检查通讯连接");
            }
            List<EquipmentProp> propsToWriter = new List<EquipmentProp>();
            var props = car.EquipmentProps;
            var wcsDeleteCommand = props.Find(t => t.EquipmentTypeTemplateCode == "wcsDeleteCommand");
            var wcsConfirmTaskFinish = props.Find(t => t.EquipmentTypeTemplateCode == "wcsConfirmTaskFinish");
            var wcsActionType = props.Find(t => t.EquipmentTypeTemplateCode == "wcsActionType");
            var wcsRow = props.Find(t => t.EquipmentTypeTemplateCode == "wcsRow");
            var wcsLine = props.Find(t => t.EquipmentTypeTemplateCode == "wcsLine");
            var wcsLayer = props.Find(t => t.EquipmentTypeTemplateCode == "wcsLayer");
            var wcsTaskHeaderId = props.Find(t => t.EquipmentTypeTemplateCode == "wcsTaskHeaderId");
            var wcsTaskCarId = props.Find(t => t.EquipmentTypeTemplateCode == "wcsTaskCarId");
            var wcsSwitch = props.Find(t => t.EquipmentTypeTemplateCode == "wcsSwitch");
            var hasPallet = props.Find(t => t.EquipmentTypeTemplateCode == "hasPallet");
            propsToWriter.AddRange(new List<EquipmentProp>() { wcsDeleteCommand, wcsConfirmTaskFinish, wcsActionType, wcsRow, wcsLine, wcsLayer, wcsTaskHeaderId, wcsTaskCarId, wcsSwitch, hasPallet });
            //return S7Helper.PlcSplitWrite(plc, propsToWriter, 20);
            var result = CarPLC.WriteAddress(propsToWriter);
            if (result.Success)
            {
                Logger.Log($"{car.Code} 删除成功", LogLevel.Info);
                return BllResultFactory.Sucess(null, "成功");
            }
            else
            {
                //Logger.Log($"通知WMS取货错误处理成功，但是下发{stocker}删除任务{task.Id}失败！", LogLevel.Error);
                Logger.Log($"{car.Code} 开始删除失败！", LogLevel.Error);
                return BllResultFactory.Error($"开始删除失败！");
            }
        }

        /// <summary>
        /// 小车变换模式
        /// </summary>
        /// <param name="car"></param>
        /// <returns></returns>
        private BllResult CarInfo_CommandControlMode(Equipment car)
        {
            if (CarPLC == null || CarPLC.GetConnStatus() == false)
            {
                //AddLogToUI("地址读取失败,请检查通讯连接", 2);
                return BllResultFactory.Error(null, "地址读取失败,请检查通讯连接");
            }
            var taskProp = car.EquipmentProps.FirstOrDefault(t => t.EquipmentTypeTemplateCode == "wcsControlMode");
            if (taskProp == null)
            {
                return BllResultFactory.Error(null, $"未找到小车{car.Name}的属性");
            }
            var result = CarPLC.WriteAddress(new List<EquipmentProp>() { taskProp });
            if (result.Success)
            {
                Logger.Log($"{car.Code} 模式切换成功", LogLevel.Info);
                return BllResultFactory.Sucess(null, "成功");
            }
            else
            {
                //Logger.Log($"通知WMS取货错误处理成功，但是下发{stocker}删除任务{task.Id}失败！", LogLevel.Error);
                Logger.Log($"{car.Code} 模式切换失败", LogLevel.Error);
                return BllResultFactory.Error($"模式切换失败！");
            }
        }

        #endregion 小车事件处理

        #region 组件

        /// <summary>
        /// 日志记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Logger_LogWrite(object sender, LogEventArgs args)
        {
            Dispatcher.Invoke(() =>
            {
                switch (args.LogLevel)
                {
                    case LogLevel.Info:
                    case LogLevel.Error:
                    case LogLevel.Warning:
                    case LogLevel.Success:
                        AppSession.LogService.WriteLog(args.LogLevel.ToString(), args.Content, args.LogTitle);
                        LogInfo.AddLogs(args.Content, args.LogLevel);
                        break;

                    case LogLevel.Exception:
                        AppSession.LogService.WriteExceptionLog(args.Content, args.Exception, args.LogTitle);
                        LogInfo.AddLogs(args.Content, args.LogLevel);
                        break;

                    case LogLevel.PLC:
                        AppSession.LogService.WriteLog(LogLevel.Success.ToString(), args.Content, args.LogTitle);
                        break;

                    default:
                        break;
                }
            });
        }

        #endregion 组件

        #region 记录桁车故障内容

        /// <summary>
        /// 记录桁车故障
        /// </summary>
        /// <param name="Truss"></param>
        private void SetTrussAlarm(Equipment srm)
        {
            if (srm.EquipmentProps.Find(t => t.EquipmentTypeTemplateCode == TrussNormalProps.OperationModel.ToString()).Value == SRMOperationModel.联机.GetIndexString() &&
                srm.EquipmentProps.Find(t => t.EquipmentTypeTemplateCode == TrussNormalProps.TotalError.ToString()).Value == "True")
            {
                //监控属性赋值
                var tempProps = srm.EquipmentProps.FindAll(t => t.EquipmentTypeTemplate.IsMonitor == true);
                var alarmProps = tempProps.Where(t => t.Value != t.EquipmentTypeTemplate.MonitorCompareValue).ToList();
                if (alarmProps.Count() > 0)
                {
                    // var warehousealarmResult = AppSession.Bll.GetCommonModelByConditionWithZero<WareHouseAlarm>($"where DateDiff(minute,created,getdate())<=5 and warehouseCode ='{App.WarehouseCode}'");
                    //var warehousealarmResult = AppSession.Dal.GetCommonModelByConditionWithZero<WareHouseAlarm>($"where flag= 0 ");
                    //if (!warehousealarmResult.Success)
                    //{
                    //    return;
                    //}
                    //if (warehousealarmResult.Data.Count == 0)
                    //{
                    //    //var warehouseAlarms = warehousealarmResult.Data;
                    foreach (var item in alarmProps)
                    {
                        var alarmtext = item.Equipment.Code + ":" + item.EquipmentTypeTemplate.MonitorFailure;
                        //if (warehouseAlarms.Count(t => t.AlarmContent == alarmtext) == 0)
                        //{
                        WareHouseAlarm wareHouseAlarm = new WareHouseAlarm();
                        wareHouseAlarm.Pn = "2020040901";
                        wareHouseAlarm.EquipmentCode = srm.Code;
                        wareHouseAlarm.Instruct = "WA";
                        wareHouseAlarm.EquipmentError = alarmtext;
                        wareHouseAlarm.EquipmentFailureTime = DateTime.Now;
                        wareHouseAlarm.Created = DateTime.Now;
                        wareHouseAlarm.CreatedBy = "WCS";
                        //wareHouseAlarm.WarehouseCode = AppSession.WarehouseCode;
                        wareHouseAlarm.Updated = DateTime.Now;
                        wareHouseAlarm.UpdatedBy = "WCS";
                        var res = AppSession.Dal.InsertCommonModel<WareHouseAlarm>(wareHouseAlarm);
                        if (!res.Success)
                        {
                            string s = res.Msg;
                        }
                        //}
                    }
                    //}
                    //else
                    //{
                    //    return;
                    //}
                }
            }
            else
            {
                var warehousealarmResult = AppSession.Dal.GetCommonModelByConditionWithZero<WareHouseAlarm>($"where flag= 0 ");
                if (warehousealarmResult.Success && warehousealarmResult.Data.Count > 0)
                {
                    //var warehouseAlarms = warehousealarmResult.Data;
                    foreach (var item in warehousealarmResult.Data)
                    {
                        //item.Flag = 0;
                        string sql = $" update wcswarehousealarm set flag = 1 ,equipmentEndFailureTime ='{DateTime.Now}'  where id={item.Id}";
                        AppSession.Dal.ExcuteCommonSqlForInsertOrUpdate(sql);
                    }
                }
                else
                {
                    return;
                }
            }
        }

        #endregion 记录桁车故障内容

        #region 时钟逻辑

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (locker)
            {
                EquipmentDispatch();
            }
        }

        /// <summary>
        /// 设备调度
        /// </summary>
        private void EquipmentDispatch()
        {
            try
            {
                #region 是否结束处理

                if (stop)
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (this.Btn_BeginExcute.IsEnabled == false || this.Btn_EndExcute.IsEnabled == true)
                        {
                            this.Btn_BeginExcute.IsEnabled = true;
                            this.Btn_EndExcute.IsEnabled = false;
                            //timer.Enabled = false;
                        }
                    });
                    return;
                }

                #endregion 是否结束处理

                #region 检查是否连接正常

                //注意此处使用过滤后的tempEquipments
                List<Equipment> tempEquipments = new List<Equipment>();
                //List<Equipment> tempEquipments = Equipments;
                var ips = Equipments.Select(t => t.IP).Distinct();
                foreach (var item in ips)
                {
                    if (PLC.GetConnectStatus(item).Success)
                    {
                        tempEquipments.AddRange(Equipments.Where(t => t.IP == item));
                    }
                    else
                    {
                        Logger.Log($"与IP【{item}】的PLC失去通讯连接，请检查设备！", LogLevel.Warning);
                    }
                }

                if (tempEquipments.Count == 0)
                {
                    Logger.Log($"当前没有已连接的设备!", LogLevel.Warning);
                    return;
                }

                #endregion 检查是否连接正常

                #region PLC心跳是否正常

                var heartbeats = tempEquipments.Where(t => t.EquipmentType.Code == SysConst.PLCHeartbeat.ToString()).ToList();
                foreach (var item in heartbeats)
                {
                    var readProp = item.EquipmentProps.Find(t => t.EquipmentTypeTemplateCode == "PLCWrite");
                    var readResult = PLC.Read(readProp);
                    if (readResult.Success && readProp.Value == "True")
                    {
                        //回复一个
                        var prop = item.EquipmentProps.Find(t => t.EquipmentTypeTemplateCode == "WCSWrite");
                        prop.Value = "True";
                        var writeResult = PLC.Write(prop);
                        if (!writeResult.Success)
                        {
                            //表示心跳断开,排除其IP对应的设备
                            tempEquipments = tempEquipments.Where(t => t.IP != item.IP).ToList();
                            Logger.Log($"写入{item.Name}的PLC心跳失败，请检查设备！", LogLevel.Warning);
                            stop = true;
                            return;
                        }
                    }
                    else
                    {
                        //表示心跳断开,排除其IP对应的设备
                        tempEquipments = tempEquipments.Where(t => t.IP != item.IP).ToList();
                        Logger.Log($"读取{item.Name}的PLC心跳失败，请检查设备！", LogLevel.Warning);
                        //return;
                    }
                }

                //无可用设备时，结束处理
                if (tempEquipments.Count == 0)
                {
                    return;
                }

                ////桁车为了防撞，必须全部连接上，不然跳过本次执行，并且给其他桁架继续发送心跳
                //if (tempEquipments.Count(t => t.EquipmentTypeId == 1) < Equipments.Count(t => t.EquipmentTypeId == 1))
                //{
                //    foreach (var trussExcute in Trusss)
                //    {
                //        foreach (var item in tempEquipments.Where(t => t.EquipmentType.Id == trussExcute.EquipmentType.Id))
                //        {
                //            trussExcute.Heartbeat(item, PLC);
                //        }
                //    }
                //    Logger.Log($"有未连接上的桁车设备，为了防止桁车相撞，尝试重新连接！", LogLevel.Warning);
                //    return;
                //}

                #endregion PLC心跳是否正常

                //筛选需要的设备属性
                var tempEquipmentProps = tempEquipments.SelectMany(t => t.EquipmentProps).Where(a => a.EquipmentTypeTemplate.PropType == EquipmentPropType.PLC_Read.ToString()).ToList();
                //读取所有PLC地址值
                var plcResult = PLC.Reads(tempEquipmentProps);
                if (!plcResult.Success)
                {
                    PLC?.DisConnect();
                    Logger.Log($"读取PLC地址错误：{plcResult.Msg},处理停止，请检查网络配置", LogLevel.Error);
                    stop = true;
                    start = false;
                    return;
                }

                start = true;

                //更新到数据库中（按需）
                //AppSession.Dal.ExcuteCommonSqlForInsertOrUpdate($"update equipment_prop set value = @value where id = @id", tempEquipments.SelectMany(t => t.EquipmentProps).Where(a => a.EquipmentTypeTemplate.PropType == "PLC").Select(t => new { value = t.Value, id = t.Id }).ToList());

                #region 设备模型监控

                Dispatcher.Invoke(() =>
                {
                    //输送线监控赋值
                    PipeLineModels.ForEach(t => t.SetProp(tempEquipments.FirstOrDefault(a => a.Name == t.Name)?.EquipmentProps));
                });

                //小车监控赋值
                Dispatcher.Invoke(() =>
                {
                    foreach (var item in CarPanel.Children)
                    {
                        if (item is CarInfo carTemp)
                        {
                            var equipments = Equipments.Where(t => t.Disable == false);
                            carTemp.SetProps(equipments);
                        }
                    }
                });

                //定长切割监控赋值
                Dispatcher.Invoke(() =>
                {
                    foreach (var item in CuttingMonitorInfoPanel.Children)
                    {
                        if (item is CuttingMonitorInfo cuttingTemp)
                        {
                            var equipments = tempEquipments.Find(t => t.Name == cuttingTemp.ControlName);
                            if (equipments == null)
                            {
                                continue;
                            }
                            cuttingTemp.SetCuttingMonitorProps(equipments);
                        }
                    }
                });

                // 测长监控赋值
                Dispatcher.Invoke(() =>
                {
                    foreach (var item in MeasuringLengthPanel.Children)
                    {
                        if (item is MeasuringLengthMonitorInfo cuttingTemp)
                        {
                            var equipments = tempEquipments.Find(t => t.Name == cuttingTemp.ControlName);
                            if (equipments == null)
                            {
                                continue;
                            }
                            cuttingTemp.SetMeasuringLengthMonitorProps(equipments);
                        }
                    }
                });

                // 坡口监控赋值
                Dispatcher.Invoke(() =>
                {
                    foreach (var item in BevelMonitorPanel.Children)
                    {
                        if (item is BevelMonitorInfo bevelTemp)
                        {
                            var equipments = tempEquipments.Find(t => t.Name == bevelTemp.ControlName);
                            if (equipments == null)
                            {
                                continue;
                            }
                            bevelTemp.SetBevelingMonitorProps(equipments);
                        }
                    }
                });

                //组队监控赋值
                Dispatcher.Invoke(() =>
                {
                    foreach (var item in AeesmblyPanel.Children)
                    {
                        if (item is AeesmblyMonitor temp)
                        {
                            var Aeesmbly = tempEquipments.Find(t => t.Name == temp.ControlName);
                            if (Aeesmbly == null)
                            {
                                continue;
                            }
                            temp.SetRobotForAeesmblyProps(Aeesmbly);
                        }
                    }
                });

                #endregion 设备模型监控

                #region 设备调度处理

                MachineExcutes.ForEach(t =>
                {
                    t.Excute(tempEquipments.Where(x => x.EquipmentType.Id == t.EquipmentType.Id).ToList(), tempEquipments, PLC);
                });

                PipeLineExcutes.ForEach(t =>
                {
                    t.Excute(tempEquipments.Where(x => x.EquipmentType.Id == t.EquipmentType.Id).ToList(), tempEquipments, PLC);
                });

                CarExcutes.ForEach(t =>
                {
                    t.Excute(tempEquipments.Where(x => x.EquipmentType.Id == t.EquipmentType.Id).ToList(), tempEquipments, PLC);
                });

                #endregion 设备调度处理

                #region 注释

                //Dispatcher.Invoke(() =>
                //{
                //    StationModels.ForEach(t => t.SetProp(tempEquipments.FirstOrDefault(a => a.Name == t.Name)?.EquipmentProps));
                //});

                //StationExcutes.ForEach(t =>
                //{
                //    t.Excute(tempEquipments.Where(a => a.EquipmentType.Id == t.EquipmentType.Id).ToList(), tempEquipments, PLC);
                //});

                //RobotExcutes.ForEach(t =>
                //{
                //    t.Excute(tempEquipments.Where(a => a.EquipmentType.Id == t.EquipmentType.Id).ToList(), tempEquipments, PLC);
                //});

                #endregion 注释

                //给与200毫秒缓冲时间
                Thread.Sleep(200);
            }
            catch (Exception ex)
            {
                //发生异常，结束处理
                stop = true;
                Logger.Log($"程序处理出现异常：{ex.Message}；", LogLevel.Exception, ex);
            }
        }

        /// <summary>
        /// 获取设备对应的PLC数据
        /// </summary>
        private List<Equipment> GetEquipmentValue()
        {
            List<Equipment> tempEquipments = new List<Equipment>();
            try
            {
                #region 检查是否连接正常

                var ips = Equipments.Select(t => t.IP).Distinct();
                foreach (var item in ips)
                {
                    if (PLC.GetConnectStatus(item).Success)
                    {
                        tempEquipments.AddRange(Equipments.Where(t => t.IP == item));
                    }
                    else
                    {
                        Logger.Log($"与IP【{item}】的PLC失去通讯连接，请检查设备！", LogLevel.Warning);
                    }
                }

                if (tempEquipments.Count == 0)
                {
                    Logger.Log($"当前没有已连接的设备!", LogLevel.Warning);
                    return tempEquipments;
                }

                #endregion 检查是否连接正常

                #region 读取PLC数据

                //筛选需要的设备属性，注意此处使用过滤后的tempEquipments
                var tempEquipmentProps = tempEquipments.SelectMany(t => t.EquipmentProps).Where(a => a.EquipmentTypeTemplate.PropType == EquipmentPropType.PLC_Read.ToString()).ToList();
                //读取所有PLC地址值
                var plcResult = PLC.Reads(tempEquipmentProps);
                if (!plcResult.Success)
                {
                    PLC?.DisConnect();
                    Logger.Log($"读取PLC地址错误：{plcResult.Msg},处理停止，请检查网络配置", LogLevel.Error);
                }

                #endregion 读取PLC数据
            }
            catch (Exception ex)
            {
                //发生异常，结束处理
                stop = true;
                Logger.Log($"程序处理出现异常：{ex.Message}；", LogLevel.Exception, ex);
            }
            return tempEquipments;
        }

        /// <summary>
        /// 监控执行状态
        /// </summary>
        /// <param name="stop"></param>
        private void ExcuteStatusInfo(bool stop)
        {
            Dispatcher.Invoke(() =>
            {
                if (stop)
                {
                    if (((SolidColorBrush)Btn_BeginExcute.Background)?.Color != Colors.Red)
                    {
                        Btn_BeginExcute.Background = new SolidColorBrush(Colors.Red);
                    }
                }
                else
                {
                    if (((SolidColorBrush)Btn_BeginExcute.Background)?.Color != Colors.Green)
                    {
                        Btn_BeginExcute.Background = new SolidColorBrush(Colors.Green);
                    }
                }
            });
        }

        /// <summary>
        /// 把PLC的值写入数据库
        /// </summary>
        /// <param name="stop"></param>
        private void PlcToPc(bool stop)
        {
            try
            {
                if (stop)
                {
                    return;
                }
                //注意此处使用过滤后的tempEquipments
                List<Equipment> tempEquipments = new List<Equipment>();
                var ips = Equipments.Select(t => t.IP).Distinct();
                foreach (var item in ips)
                {
                    if (PLC.GetConnectStatus(item).Success)
                    {
                        tempEquipments.AddRange(Equipments.Where(t => t.IP == item));
                    }
                }
                if (tempEquipments.Count > 0)
                {
                    var PropType = new string[] { EquipmentPropType.PLC_Read.ToString(), EquipmentPropType.PLC_DelayRead.ToString(), EquipmentPropType.PLC_BackgroundRead.ToString() };
                    // 获取要读取的设备属性
                    var tempEquipmentProps = tempEquipments.SelectMany(t => t.EquipmentProps).Where(x => PropType.Contains(x.EquipmentTypeTemplate.PropType)).ToList();
                    // 读取所有PLC地址值
                    var plcResult = PLC.Reads(tempEquipmentProps);
                    if (plcResult.Success)
                    {
                        // 更新到数据库中
                        AppSession.Dal.ExcuteCommonSqlForInsertOrUpdate($"update equipment_prop set value = @value where id = @id", tempEquipmentProps.Select(t => new { value = t.Value, id = t.Id }).ToList());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"同步PLC到数据库出现异常：{ex.Message}；", LogLevel.Exception, ex);
            }
        }

        #endregion 时钟逻辑

        #region 窗体事件

        //private void InitTimer()
        //{
        //    timer.Interval = App.Interval; //1秒触发一次
        //    timer.Elapsed += Timer_Elapsed; ;
        //    timer.AutoReset = true;//每到指定时间Elapsed事件是到时间就触发
        //    timer.Enabled = false; //指示 Timer 是否应引发 Elapsed 事件。
        //}

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var childMenus = AppSession.BllService.GetAllMenuOperation().Data.Where(t => (t.MenuType == "catalog" || t.MenuType == "menu") && App.MenuOperations.Count(a => a.Id == t.Id) > 0).ToList();
            AppSession.BllService.Combine(App.MenuOperations.Where(t => t.ParentId == null).ToList(), childMenus);
            MenuMain.ItemsSource = App.MenuOperations.Where(t => t.ParentId == null).OrderBy(t => t.OrderNum).ToList();

            #region 开启打标机服务器的监听 //cf

            if (IsOpen == false)
            {
                SendDataToMarking.Start();
                IsOpen = true;
            }

            #endregion 开启打标机服务器的监听 //cf

            //初始化
            Init();
        }

        private void MenuMain_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuOperation menu = (MenuOperation)((MenuItem)e.OriginalSource).Header;
                var win = ChildrenWin.FirstOrDefault(t => t.Key == menu.Url).Value;
                if (String.IsNullOrWhiteSpace(menu?.Url) || menu.Url == "#")
                {
                    return;
                }
                if (win == null)
                {
                    win = (Window)Activator.CreateInstance(null, menu.Url).Unwrap();
                    win.Owner = this;
                    ChildrenWin.Add(menu.Url, win);
                }
                win.Show();
                if (win.WindowState == WindowState.Minimized)
                    win.WindowState = WindowState.Normal;
                win.Activate();
            }
            catch (Exception ex)
            {
                AppSession.LogService.WriteExceptionLog("打开菜单", ex);
                MessageBox.Show("打开菜单出现异常！");
            }
        }

        private void Btn_BeginExcute_Click(object sender, RoutedEventArgs e)
        {
            if (PLC == null)
            {
                MessageBox.Show("PLC连接初始化失败！ 请检查PLC配置然后重启本程序", "注意", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (PLC.GetConnectStatus().Success == false)
            {
                var result = PLC.Connect();
                if (!result.Success)
                {
                    MessageBox.Show($"打开PLC连接失败：{result.Msg}");
                    return;
                }
            }
            stop = false;
            //timer.Enabled = true;
            Btn_BeginExcute.IsEnabled = false;
            Btn_EndExcute.IsEnabled = true;
        }

        private void Btn_EndExcute_Click(object sender, RoutedEventArgs e)
        {
            stop = true;
            Btn_BeginExcute.IsEnabled = true;
            Btn_EndExcute.IsEnabled = false;
            //1秒后断开连接
            Thread.Sleep(1000);
            PLC?.DisConnect();
        }

        private void BaseWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!stop)
            {
                MessageBox.Show("请先停止处理，再关闭本程序");
                e.Cancel = true;
                return;
            }
            //timer.Enabled = false;
            Thread.Sleep(1000);
            Application.Current.Shutdown();
        }

        #endregion 窗体事件

        #region 套料方案导入数据库

        private CSVFile cSVFile = new CSVFile();

        private BllResult importExcelOrder()
        {
            try
            {
                Random rad = new Random();//实例化随机数产生器rad；
                var dateresult = cSVFile.OpenCSVFile();

                if (dateresult == null)
                {
                    return BllResultFactory.Error("未能成功打开CSV文件");
                }

                if (dateresult.Columns.Count != 8)
                {
                    return BllResultFactory.Error("CSV文件列数不正确，无法导入");
                }
                for (int i = 0; i < dateresult.Rows.Count; i++)
                {
                    string ZuDuiCheck = dateresult.Rows[i][7].ToString().Replace(" ", "");
                    string LengthCheck = dateresult.Rows[i][5].ToString().Replace(" ", "");
                    if (ZuDuiCheck != "1" && ZuDuiCheck != "2" && ZuDuiCheck != "3" && ZuDuiCheck != "")
                    {
                        return BllResultFactory.Error("模板中组对平台值错误,平台值只能为1、2、3和空");
                    }
                    if (Convert.ToInt32(LengthCheck) > 1000 && dateresult.Rows[i][7].ToString().Replace(" ", "") == "")
                    {
                        return BllResultFactory.Error("管子长度超过一米,平台值不能为空");
                    }
                }

                for (int i = 0; i < dateresult.Rows.Count; i++)
                {
                    //外径
                    string WaiJing = dateresult.Rows[i][0].ToString().Replace(" ", "");
                    //壁厚
                    string BiHou = dateresult.Rows[i][1].ToString().Replace(" ", "");
                    //序号
                    string XuHao = dateresult.Rows[i][2].ToString().Replace(" ", "");
                    //分段名
                    string FenDuanMing = dateresult.Rows[i][3].ToString().Replace(" ", "");
                    //管名
                    string GuanMing = dateresult.Rows[i][4].ToString().Replace(" ", "");
                    //切断长
                    string Length = dateresult.Rows[i][5].ToString().Replace(" ", "");
                    //管段
                    string GuanDuan = dateresult.Rows[i][6].ToString().Replace(" ", "");
                    //组对工位
                    string ZuDui = dateresult.Rows[i][7].ToString().Replace(" ", "");
                    //
                    int stationCacheId = rad.Next(1000, 10000);//暂定生成4位随机数作为原材料管子唯一标识ID
                    //continue循环

                    if (!decimal.TryParse(WaiJing, out decimal diameter))
                    {
                        continue;
                    }
                    if (!decimal.TryParse(BiHou, out decimal thickness))
                    {
                        continue;
                    }
                    if (!int.TryParse(Length, out int lengthInt))
                    {
                        continue;
                    }
                    if (!int.TryParse(ZuDui, out int AssemblyStation))
                    {
                        continue;
                    }
                    if (!int.TryParse(XuHao, out int serialNumber))
                    {
                        continue;
                    }

                    //将表格中内容写入数据库
                    try
                    {
                        CutPlan cutPlan = new CutPlan();
                        cutPlan.SerialNumber = Convert.ToInt32(serialNumber);
                        cutPlan.SectionName = FenDuanMing.ToString();
                        cutPlan.PipeName = GuanMing.ToString();
                        cutPlan.PipeEnd = GuanDuan.ToString();
                        cutPlan.Diameter = diameter;
                        cutPlan.Thickness = thickness;
                        cutPlan.Length = lengthInt;
                        cutPlan.AssemblyStation = AssemblyStation;
                        cutPlan.Status = StationCacheStatus.初始.GetIndexInt();
                        cutPlan.stationCacheId = stationCacheId;
                        var insertResult = AppSession.Dal.InsertCommonModel<CutPlan>(cutPlan);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        continue;
                    }
                }
                MessageBox.Show("套料结果发送到数据库成功！");
                return BllResultFactory.Sucess("导入Excel工单成功！");
            }
            catch (Exception ex)
            {
                return BllResultFactory.Error($"导入Excel工单发生异常：{ex.Message}");
            }
        }

        #endregion 套料方案导入数据库

        private void Btn_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var result = importExcelOrder();
            if (!result.Success)
            {
                MessageBox.Show(result.Msg);
            }
        }

        private CutterNormalExcute cut = new CutterNormalExcute();
        public string diameter { get; set; }

        /// <summary>
        /// 请求大径切割PLC
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonBig_Click(object sender, RoutedEventArgs e)
        {
            var tempEquipments = GetEquipmentValue();
            var cuttingBatchBig = Equipments.FirstOrDefault(t => t.Code == "CuttingBatchBig");
            var Request = cuttingBatchBig.EquipmentProps.FirstOrDefault(t => t.EquipmentTypeTemplateCode == "Request");
            var Reply = cuttingBatchBig.EquipmentProps.FirstOrDefault(t => t.EquipmentTypeTemplateCode == "Reply");
            try
            {
                Request.Value = "1";
                //var sendResult = PLC.Write(Request);
                //if (!sendResult.Success)
                //{
                //    Logger.Log($"向设备[{cuttingBatchBig.Name}]写入 请求失败，请检查设备！", LogLevel.Warning);
                //    return;
                //}
                //Thread.Sleep(100);
                //var readResult = PLC.Read(Reply);
                //if (!readResult.Success)
                //{
                //    Logger.Log($"向设备[{cuttingBatchBig.Name}] 读取PLC是否允许信号失败，原因：{readResult.Msg}！", LogLevel.Error);
                //    return;
                //}
                //if (Reply.Value == "2")
                //{
                //    Logger.Log($"向设备[{cuttingBatchBig.Name}]写入 请求成功，但是设备不允许写入套料数据！", LogLevel.Error);
                //    return;
                //}
                cut.ExcuteCutZC(cuttingBatchBig, PLC, "Big");
                //Request.Value = "0";
                //PLC.Write(Request);
            }
            catch (Exception ex)
            {
            }
        }

        public void ImportPlcAddress()
        {
            #region plc地址入库

            EquipmentTypeTemplate MaterialID = new EquipmentTypeTemplate();
            EquipmentTypeTemplate MaterialLength = new EquipmentTypeTemplate();
            EquipmentTypeTemplate Diameter = new EquipmentTypeTemplate();
            EquipmentTypeTemplate Thickness = new EquipmentTypeTemplate();
            EquipmentTypeTemplate Length = new EquipmentTypeTemplate();
            EquipmentTypeTemplate PipeEnd = new EquipmentTypeTemplate();
            EquipmentTypeTemplate AssemblyStation = new EquipmentTypeTemplate();

            for (int i = 1; i <= 1000; i++)
            {
                ///原材料ID
                MaterialID.Code = "WCSCutMaterialID_" + i.ToString();
                MaterialID.Name = "WCS回复切割-原材料ID";
                MaterialID.Description = "WCS回复切割-原材料ID";
                MaterialID.EquipmentTypeId = 1021;
                MaterialID.PropType = EquipmentPropType.PLC_NotRead.ToString();
                MaterialID.DataType = "int".ToUpper();
                MaterialID.Offset = ((i - 1) * 14).ToString();
                MaterialID.IsMonitor = true;

                ///原材料长度
                MaterialLength.Code = "WCSCutMaterialLength_" + i.ToString();
                MaterialLength.Name = "WCS回复切割-原材料长度";
                MaterialLength.Description = "WCS回复切割-原材料长度";
                MaterialLength.EquipmentTypeId = 1021;
                MaterialLength.PropType = EquipmentPropType.PLC_NotRead.ToString();
                MaterialLength.DataType = "int".ToUpper();
                MaterialLength.Offset = ((i - 1) * 14 + 2).ToString();
                MaterialLength.IsMonitor = true;
                ///原材料直径
                Diameter.Code = "WCSCutDiameter_" + i.ToString();
                Diameter.Name = "WCS回复切割-原材料直径";
                Diameter.Description = "WCS回复切割-原材料直径";
                Diameter.EquipmentTypeId = 1021;
                Diameter.PropType = EquipmentPropType.PLC_NotRead.ToString();
                Diameter.DataType = "int".ToUpper();
                Diameter.Offset = ((i - 1) * 14 + 4).ToString();
                Diameter.IsMonitor = true;
                ///原材料壁厚
                Thickness.Code = "WCSCutThickness_" + i.ToString();
                Thickness.Name = "WCS回复切割-原材料壁厚";
                Thickness.Description = "WCS回复切割-原材料壁厚";
                Thickness.EquipmentTypeId = 1021;
                Thickness.PropType = EquipmentPropType.PLC_NotRead.ToString();
                Thickness.DataType = "int".ToUpper();
                Thickness.Offset = ((i - 1) * 14 + 6).ToString();
                Thickness.IsMonitor = true;
                ///切断长
                Length.Code = "WCSCutLength_" + i.ToString();
                Length.Name = "WCS回复切割-切断长度";
                Length.Description = "WCS回复切割-切断长度";
                Length.EquipmentTypeId = 1021;
                Length.PropType = EquipmentPropType.PLC_NotRead.ToString();
                Length.DataType = "int".ToUpper();
                Length.Offset = ((i - 1) * 14 + 8).ToString();
                Length.IsMonitor = true;
                ///管端
                PipeEnd.Code = "WCSCutPipeEnd_" + i.ToString();
                PipeEnd.Name = "WCS回复切割-管端";
                PipeEnd.Description = "WCS回复切割 - 管端";
                PipeEnd.EquipmentTypeId = 1021;
                PipeEnd.PropType = EquipmentPropType.PLC_NotRead.ToString();
                PipeEnd.DataType = "int".ToUpper();
                PipeEnd.Offset = ((i - 1) * 14 + 10).ToString();
                PipeEnd.IsMonitor = true;
                ///下料工位
                AssemblyStation.Code = "WCSCutAssemblyStation_" + i.ToString();
                AssemblyStation.Name = "WCS回复切割 - 下料工位";
                AssemblyStation.Description = "WCS回复切割-下料工位";
                AssemblyStation.EquipmentTypeId = 1021;
                AssemblyStation.PropType = EquipmentPropType.PLC_NotRead.ToString();
                AssemblyStation.DataType = "int".ToUpper();
                AssemblyStation.Offset = ((i - 1) * 14 + 12).ToString();
                AssemblyStation.IsMonitor = true;

                ///分别入库
                var a = AppSession.Dal.InsertCommonModel<EquipmentTypeTemplate>(MaterialID);
                var b = AppSession.Dal.InsertCommonModel<EquipmentTypeTemplate>(MaterialLength);
                var c = AppSession.Dal.InsertCommonModel<EquipmentTypeTemplate>(Diameter);
                var d = AppSession.Dal.InsertCommonModel<EquipmentTypeTemplate>(Thickness);
                var h = AppSession.Dal.InsertCommonModel<EquipmentTypeTemplate>(Length);
                var f = AppSession.Dal.InsertCommonModel<EquipmentTypeTemplate>(PipeEnd);
                var g = AppSession.Dal.InsertCommonModel<EquipmentTypeTemplate>(AssemblyStation);
            }

            #endregion plc地址入库
        }

        private void buttonCentre_Click(object sender, RoutedEventArgs e)
        {
            var tempEquipments = GetEquipmentValue();
            var cuttingBatcCentre = Equipments.FirstOrDefault(t => t.Code == "CuttingBatchCentre");
            var Request = cuttingBatcCentre.EquipmentProps.FirstOrDefault(t => t.EquipmentTypeTemplateCode == "Request");
            var Reply = cuttingBatcCentre.EquipmentProps.FirstOrDefault(t => t.EquipmentTypeTemplateCode == "Reply");
            try
            {
                Request.Value = "1";
                var sendResult = PLC.Write(Request);
                if (!sendResult.Success)
                {
                    Logger.Log($"向设备[{cuttingBatcCentre.Name}]写入 请求失败，请检查设备！", LogLevel.Warning);
                    return;
                }
                Thread.Sleep(100);
                var readResult = PLC.Read(Reply);
                if (!readResult.Success)
                {
                    Logger.Log($"向设备[{cuttingBatcCentre.Name}] 读取PLC是否允许信号失败，原因：{readResult.Msg}！", LogLevel.Error);
                    return;
                }
                if (Reply.Value == "2")
                {
                    Logger.Log($"向设备[{cuttingBatcCentre.Name}]写入 请求成功，但是设备不允许写入套料数据！", LogLevel.Error);
                    MessageBox.Show("存在未切完的方案，不能再次请求切割数据！");
                    return;
                }
                cut.ExcuteCutZC(cuttingBatcCentre, PLC, "Centre");
                Request.Value = "0";
                PLC.Write(Request);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
