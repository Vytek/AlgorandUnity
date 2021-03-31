using UnityEngine;
using Random = System.Random;

namespace MHLab.Utilities
{
    public class Test : MonoBehaviour
    {
        public BackgroundTasksProcessor Processor;

        public GameObject Cube;

        private float  _timer;
        private Random _random;

        private void Awake()
        {
            _random = new Random();
        }

        private void Update()
        {
            if (!Processor.IsReady)
            {
                return;
            }

            _timer += Time.deltaTime;

            if (_timer >= 1f)
            {
                Processor.Process(
                    () =>
                    {
                        var position = new Vector3(
                            _random.Next(-5, 5),
                            0,
                            _random.Next(-5, 5)
                        );
                        
                        return position;
                    },
                    (result) =>
                    {
                        var position = (Vector3)result;
                        Cube.transform.position = position;
                    }
                );

                _timer = 0;
            }
        }
    }
}