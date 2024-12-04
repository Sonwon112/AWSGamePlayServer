using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NetworkPlayServer
{
    internal class GameManager
    {
        public static GameManager Instance = new GameManager();
        private int totalCnt = 2;
        private int currentCnt = 0;

        public GameManager() {
        
        }

        /// <summary>
        /// 참가자 추가시 현재 인원수를 추가
        /// </summary>
        /// <returns>현재 인원수와 총인원수가 같이지면 true, 그 외 false</returns>
        public bool currCntUp() {
            currentCnt += 1;
            Console.WriteLine("participant" + currCntUp + "/" + totalCnt);
            if (currentCnt == totalCnt)
            {
                return true;
            }
            return false;
        }

        public int getCurrCnt()
        {
            return currentCnt;
        }
        public int getTotalCnt()
        {
            return totalCnt;
        }
    }
}
