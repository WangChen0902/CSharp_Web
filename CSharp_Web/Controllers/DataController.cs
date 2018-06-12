using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Serialization.Json;
using System.IO;
using CSharp_Web.Models;
using CSharp_Web.Serial;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CSharp_Web.Controllers
{
    [Route("api/[controller]")]
    public class DataController : Controller
    {
        public MyPort myPort = new MyPort();
        private readonly DataContext _context;
        List<Data> saveData = new List<Data>();

        public DataController(DataContext context)
        {
            _context = context;
            int count = _context.Datas.Count();
            if (_context.Datas.Count()==0)
            {
                List<Data> dataList = new List<Data>();
                FileStream fs = new FileStream("D:\\temp\\info.json", FileMode.Open);
                DataContractJsonSerializer ser = new DataContractJsonSerializer(dataList.GetType());
                dataList = ser.ReadObject(fs) as List<Data>;
                fs.Close();
                foreach(Data data in dataList)
                {
                    _context.Datas.Add(data);
                }
                _context.SaveChanges();
            }
        }

        [HttpGet]
        public IEnumerable<Data> GetAll()
        {
            Console.WriteLine("HTTP Get all!");
            return _context.Datas.ToList();
        }

        [HttpGet("{ID}", Name = "GetData")]
        public IActionResult GetByID(int id)
        {
            Console.WriteLine("HTTP GetBy ID!");

            var data = _context.Datas.FirstOrDefault(t => t.ID == id);
            if (data == null)
            {
                return NotFound();
            }
            return new ObjectResult(data);
        }

        [HttpPost]
        public IActionResult Create([FromBody] Data data)
        {
            Console.WriteLine($"HTTP Post(Add) Data {data.ID}!");

            if (data==null)
            {
                return BadRequest();
            }
            _context.Datas.Add(data);
            _context.SaveChanges();
            myPort.WriteLED(data.Red, data.Green, data.Yellow, data.Blue, data.White);
            if (data.Temperature == -1 && data.Light == -1)
            {
                saveData.Clear();
                Read(data);
                FileStream fs = new FileStream("D:\\temp\\save.json", FileMode.Open);
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<Data>));
                ser.WriteObject(fs, saveData);
                fs.Close();
            }
            myPort.Close();
            return CreatedAtRoute("GetData", new { id = data.ID }, data);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Data data)
        {
            Console.WriteLine($"HTTP Put(Update) Data {id}!");
            if (data == null || data.ID != id)
            {
                return BadRequest();
            }
            var myData = _context.Datas.FirstOrDefault(t => t.ID == id);
            if(myData==null)
            {
                return NotFound();
            }
            myData.Port = data.Port;
            myData.Rate = data.Rate;
            myData.Temperature = data.Temperature;
            myData.Light = data.Temperature;
            myData.Red = data.Red;
            myData.Green = data.Green;
            myData.Yellow = data.Yellow;
            myData.Blue = data.Blue;
            myData.White = data.White;
            myData.Send = data.Send;
            myData.Recv = data.Recv;

            _context.Datas.Update(myData);
            _context.SaveChanges();
            return new NoContentResult();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            Console.WriteLine($"HTTP Delete Data {id}!");

            var data =  _context.Datas.FirstOrDefault(t => t.ID == id);
            if(data==null)
            {
                return NotFound();
            }
            _context.Datas.Remove(data);
            _context.SaveChanges();
            return new NoContentResult();
        }

        public Task<Data> ReadData(int id)
        {
            Data tempData = new Data();
            return Task.Run(() => 
            {
                tempData = myPort.GetData(id);
                return tempData;
            });
        }

        public async void Read(Data data)
        {
            for (int i = 0; i < int.Parse(data.Recv); i++)
            {
                Data tempData = await ReadData(data.ID + i +1);
                saveData.Add(tempData);
            }
        }
    }
}
