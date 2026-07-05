using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedKernel.Common.Wrappers.Response;

public class ResponseListDto<T> where T : class
{
    public Meta meta { get; set; } = new Meta();


    public List<T> data { get; set; } = new List<T>();


    public ResponseListDto()
    {
    }

    public ResponseListDto(Meta _meta, List<T> _data)
    {
        meta = _meta;
        data = _data;
    }
}
public class Meta
{
    public int page { get; set; }

    public int page_size { get; set; }

    public Ranger ranger { get; set; } = new Ranger();

    public int total { get; set; }

    public int total_page { get; set; }

    public Meta()
    {
    }

    public Meta(int _page, int _page_size, int _total)
    {
        ranger = new Ranger();
        page = _page;
        page_size = _page_size;
        total = _total;
        if (_page_size == 0 || _page == 0)
        {
            total_page = 1;
            ranger.from = _total != 0 ? 1 : 0;
            ranger.to = _total;
            return;
        }

        total_page = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(_total) / Convert.ToDouble(_page_size)));
        ranger.from = _total != 0 ? (_page - 1) * _page_size + 1 : 0;
        ranger.to = (_page - 1) * _page_size + _page_size;
        if (_total < ranger.to)
        {
            ranger.to = _total;
        }

        if (total_page < page)
        {
            ranger.from = 0;
            ranger.to = 0;
        }
    }
}
public class Ranger
{
    public int from { get; set; }

    public int to { get; set; }
}
