using Fenrir.Multiplayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fenrir.ECS
{
    public class InputBuffer<TInput> where TInput : struct
    {
        private Dictionary<int, TInput> _playerInputs;

        public Dictionary<int, TInput> Inputs => _playerInputs;

        public InputBuffer() 
        {
            _playerInputs = new Dictionary<int, TInput>();
        }

        public void SetInput(int playerId, TInput input)
        {
            _playerInputs[playerId] = input;
        }

        public TInput GetInput(int playerId)
        {
            return _playerInputs[playerId];
        }

        public bool TryGetInput(byte playerId, out TInput input)
        {
            return _playerInputs.TryGetValue(playerId, out input);
        }


        public void CopyFrom(InputBuffer<TInput> other)
        {
            _playerInputs.Clear();

            foreach (var kvp in other.Inputs)
            {
                _playerInputs[kvp.Key] = kvp.Value;
            }
        }
    }
}
