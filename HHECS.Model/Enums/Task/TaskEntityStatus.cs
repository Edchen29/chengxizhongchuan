﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HHECS.Model.Enums.Task
{
    public enum TaskEntityStatus
    {
        任务创建 = 1,
        下发任务 = 10,
        待机=0,
        //执行中=111,
        //多任务堆垛机状态
        下发堆垛机库内取货 = 20,//（开始执行“出”性质任务，去目标库位取出托盘）
        响应堆垛机库内取货完成 = 25,//（“出”性质任务完成，此时这个任务对应的托盘在货叉内）
        下发堆垛机库外放货 = 30,//（“出”性质任务放到对应接出口）
        响应堆垛机库外放货完成 = 35,//（堆垛机将托盘已经放到接出口）
        响应接出口站台请求 = 40,//（此时将任务写给站台完毕）
        到达拣选站台 = 50,//（响应拣选站台的位置到达）--整出任务直接完成
        拣选台回库 = 60,//（模拟电气按钮或是响应地址请求后）
        响应接入站台到达 = 70,//可被堆垛机执行入库任务了
        下发堆垛机库外取货 = 75,//（指示堆垛机去接入口接托盘）
        响应堆垛机库外取货完成 = 80,//（此时堆垛机已经接完托盘，托盘应在货叉内等待去向指令）
        下发堆垛机库内放货 = 85,//（此时堆垛机带着托盘去目标库位）--同巷道库内移库任务直接跳到这
        响应堆垛机库内放货完成 = 90,	//（此时堆垛机已经将托盘放入了目标货位）

        //单任务堆垛机状态
        下发堆垛机入库任务 = 91,
        下发堆垛机出库任务 = 93,
        下发堆垛机库内移库 = 95,
        下发堆垛机换站任务 = 98,

        任务完成 = 100, //任务完成
        //任务回传失败 = 110, //任务回传失败
        //任务回传成功 = 120, //任务回传成功

        //空出与取货错为异常结束
        异常结束 = 130,



        //agv任务状态
        下发进缓存位 = 130,
        响应进缓存位 = 130,
        下发出缓存位 = 130,
        响应出缓存位 = 130,
        下发打标任务 = 130,
        响应打标完成 = 130,
        下发去坡口缓存任务 = 130,
        响应去坡口缓存完成 = 130,
        下发去坡口任务 = 130,
        响应去坡口完成 = 130,
        下发AGV翻转任务 = 130,
        响应AGV翻转完成 = 130,
        下发去组队输送线任务 =130,
        响应去组队输送线完成 = 130,
        
    }
}