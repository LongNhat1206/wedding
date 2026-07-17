using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class SaveFormController : ControllerBase
{
    // Khai báo trực tiếp Service Account config
    private const string ServiceAccountEmail = "sheet-pusher@loginvie.iam.gserviceaccount.com";
    private const string PrivateKey = @"-----BEGIN PRIVATE KEY-----
        MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQCjw/l2dfLM7IYQ
        5T3LzMphLzF+VeMmhz+7ZBRZZXAT+JTOQp02HeY7Jfpop2v/ihQ0JWnPEvgN8Edr
        MhoiDxl0DCaKudGhl2B0Ezjm/LEAVww5QjR4qZTm6wrTHsYM+p2YSXiDOe4OF6IY
        scQOI+2gLQX0CPDd1aKTvrr51lvP5aYy7C9syCclcdtubPqK+IHo+btFi94IXN/D
        u8GuiYfFwTUeKahsWXFXNYu1SNHedHmVf1lQ+9284dZRS9P8LWKMR8MPYwlyVNRA
        2v0VYSIwJRKd73WTyG9+WO8SWgv2Y8r6kNqbjFmW9dmQq3sTxfj3psYsQ50FsUst
        cbTvIJKBAgMBAAECggEASpeqGg4RYMvfKPTx19FY4TlYxt2iEFuMaPNl3W45xa71
        QJnQ9+RYiNp8m4yewY+IMlRURJC0Ucz/CtGQW7Ea++PI7mlDFeJYeaaDxZqVmL0N
        gUspkUhU3XVzwcQDm8fPXp0gek05pEuZdrwJvBMbWlkHnCcyZyi226sFiWTc/u01
        IvjEW4ShC94pEVPxqPCcCUrX5nwdb3X7Wjav0BbkxraRIQvs5opV0qw02TYvSTVj
        LajNOluc2uZ2ffqxWZIui+qSuhjzCqlHPBBpOKyVhVFXhbu/ii+lNW+yGbOOWCO5
        6HnlygwIZ4UezTOEBGONC57/MJq9eACjzyNp3sVT3QKBgQDgJzoTFbm9HKUdYJD3
        RPEcHU2oBnCIUK6MtxDDpfXl4cAQTHmYqNhG1Mpr1WeCWvy/luIxlhhhUNRfArLQ
        r4UvsWNGvaqqFHvZZlQSlF7psPiorwo9iNm1zcTwFbQTA2kr3ieGoL2aVQvk1spy
        sIwEA/vhxdxXyhTZDiazDOnqowKBgQC7CFxMEjwPG66kNe9EDp4V4Jtm5K6cI1kf
        yhwd/UR+sJjwaAX1YVEWFxCTTYx9i7ZCRjS1vvsPNjo0Cs/eyhnjgn2nnQLT2SHi
        fru7k2N4VAd4VxCaKRNH+OZUM9Zds6UpeSc2xUUFvP/qiP5X6bny5pSP5XBv/qBz
        zhXsG5fkiwKBgQDT7eKnprPdDFdEhkepsiIwzbfddHEzQO03l+IeySvLtHyOLHAP
        GRybI5dbCwL5qaMsVbD45wuX/v878WY1jq3jINlXSf4xHnrWWjyE8IkodC0194E8
        GsaUcL+Rq0N9co0eb8V5MULyxxQewcHZW/iV11pv5U3mJuc9LXcDdOM49wKBgQCE
        Dl4ktAdF35TKiu5aTjveVI1E9Bg5VFf8MxbxMb7n+Mazj68NV14KS9S/PNUCW+Af
        ITTcUnEvh2lHD48/zdDDq4IPE7RIYhojsrnYsjNcZXA0zryBLJlZiusN7t/fnxTx
        mIJQkK6wY4cKURnMdtF9KMMiaqyd1tnxikQ+RilUCwKBgEdRQgj6lW2JK0ZzI50i
        pXRjCAem/rNwbUs47b+sjIcfNeJTmiX0AAXu+RuDeB7tVOU6xvgbq5fw4mNoLg5u
        U0jHAX9MetmUGbzDE6gITBNNLTvMB61PggCDNdEcZ6CRl9AQEtKwjHj2oDGZHGPB
        GI4X8ehk7yNBlvaU9WYSiDCv
        -----END PRIVATE KEY-----";

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] LadiFormPayload payload)
    {
        try
        {
            await SaveToGoogleSheet(payload);
            return Ok(new { code = 200, data = "", message = "Success" });
        }
        catch (Exception ex)
        {
            return Ok(new { code = 200, data = "", message = "Success" });
        }
    }



    public async Task SaveToGoogleSheet(LadiFormPayload payload)
    {
        try
        {
            var credential = new ServiceAccountCredential(
            new ServiceAccountCredential.Initializer(ServiceAccountEmail)
            {
                Scopes = new[] { SheetsService.Scope.Spreadsheets }
            }.FromPrivateKey(PrivateKey)
            );

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "LadiPageFormSaver",
            });

            var range = "'Congratulations'";
            var valueRange = new ValueRange();
            valueRange.MajorDimension = "ROWS";
            valueRange.Values = new List<IList<object>>()
            {
                new List<object>
                {
                    payload.form_data.FirstOrDefault(f => f.name == "name")?.value ?? "Khách lạ",
                    payload.form_data.FirstOrDefault(f => f.name == "message")?.value ?? (payload.form_data.FirstOrDefault(f => f.name == "form_item5")?.value ?? "Chúc mừng hạnh phúc"),
                    payload.form_data.FirstOrDefault(f => f.name == "FORM_ITEM")?.value ?? "Bạn chung"
                }
            };

            var appendRequest = service.Spreadsheets.Values.Append(valueRange, "12UEXxpUZrQbRrhMtE_w-z9Arm_QdDcTGqPH1qNxbQHA", range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            appendRequest.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

            var response = await appendRequest.ExecuteAsync();
        }
        catch (Exception ex)
        {

        }
    }
}

// Models
public class LadiFormPayload
{
    public string form_config_id { get; set; }
    public string ladi_form_id { get; set; }
    public string ladipage_id { get; set; }
    public List<TrackingItem> tracking_form { get; set; }
    public List<FormDataItem> form_data { get; set; }
    public int status_send { get; set; }
    public bool merge_address { get; set; }
    public int total_revenue { get; set; }
    public int time_zone { get; set; }
    public string event_id { get; set; }
}

public class TrackingItem
{
    public string name { get; set; }
    public string value { get; set; }
}

public class FormDataItem
{
    public string name { get; set; }
    public string value { get; set; }
}
