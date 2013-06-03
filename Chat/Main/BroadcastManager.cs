using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using Chat.Helpers;
using Chat.Main.Packet.DataTypes;

namespace Chat.Main
{   
    class BroadcastManager
    {

        private Boolean _isBusy = false;
        private int _countOfClients = 0; // Общее количество клиентов 
        private int _countOfProcessedClients = 0; // Количество обработанных килиентов

        /// <summary>
        /// Контролирует доставку широковещательных сообщений 
        /// </summary>
        /// <param name="countOfClients">Количество клиентов в системе</param>
        public BroadcastManager(int countOfClients)
        {
            _countOfClients = countOfClients;
        }

        /// <summary>
        /// Устанавливает что компонент занят отправкой сообщения. Возвращает true если удалось установить состояние занятости и false если компонент уже занят.
        /// </summary>
        /// <returns></returns>
        public Boolean SetBusy ()
        {
            if (_isBusy)
            {
                return false;
            }
            else
            {
                _isBusy = true;
                return true;
            }
        }


        public void ReceiveAcknowledge(object sender, MessageRecivedEventArgs e)
        {
            // Если получен пакет состояния доставки широковещательного сообщения и установлен флаг IsBusy 
            if ((e.MessageType == MessageType.BroadcastTextDelivered || e.MessageType == MessageType.BroadcastTextUndelivered) && _isBusy)
            {
                // Инкрементировать количество полученных пакетов и сравнить с общий количеством клиентов
                if (++_countOfProcessedClients >= _countOfClients)
                {
                    // Если достигнуто значения общего количества клиентов то установить что компонент свободен и обнулить количество обработнных пакетов
                    _isBusy = false;
                    _countOfProcessedClients = 0;
                }
            }
        }

    }
}
