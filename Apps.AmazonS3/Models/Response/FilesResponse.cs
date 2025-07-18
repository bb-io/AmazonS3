using Blackbird.Applications.Sdk.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.AmazonS3.Models.Response;
public class FilesResponse
{
    [Display("Files")]
    public IEnumerable<BucketObject> Objects { get; set; } = [];
}
