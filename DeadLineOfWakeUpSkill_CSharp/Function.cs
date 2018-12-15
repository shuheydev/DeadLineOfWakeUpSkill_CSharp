using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using AlexaPersistentAttributesManager;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace DeadLineOfWakeUpSkill_CSharp
{
    public class Function
    {
        private readonly string skillName = "DeadLineOfWakeUpSkill";
        private readonly string slot_wakeTime = "wakeTime";

        //hello
        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="skillRequest"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public SkillResponse FunctionHandler(SkillRequest skillRequest, ILambdaContext context)
        {
            SkillResponse skillResponse = null;

            try
            {
                //型スイッチの利用
                switch (skillRequest.Request)
                {
                    case LaunchRequest launchRequest:
                        skillResponse = LaunchRequestHandler(skillRequest);
                        break;
                    case IntentRequest intentRequest:
                        switch (intentRequest.Intent.Name)
                        {
                            case "SetDeadLineIntent":
                                skillResponse = SetDeadLineIntentHandler(skillRequest);
                                break;
                            case "ReportRemainTimeIntent":
                                skillResponse = ReportRemainTimeIntentHandler(skillRequest);
                                break;
                            case "AMAZON.HelpIntent":
                                skillResponse = HelpIntentHandler(skillRequest);
                                break;
                            case "AMAZON.CancelIntent":
                                skillResponse = CancelAndStopIntentHandler(skillRequest);
                                break;
                            case "AMAZON.StopIntent":
                                skillResponse = CancelAndStopIntentHandler(skillRequest);
                                break;
                            default:
                                //skillResponse = ErrorHandler(skillRequest);
                                break;
                        }

                        break;
                    case SessionEndedRequest sessionEndedRequest:
                        skillResponse = SessionEndedRequestHandler(skillRequest);
                        break;
                    default:
                        //skillResponse = ErrorHandler(skillRequest);
                        break;
                }
            }
            catch (Exception ex)
            {
                skillResponse = ErrorHandler(skillRequest);
            }

            return skillResponse;
        }




        #region 各インテント、リクエストに対応する処理を担当するメソッドたち

        private SkillResponse LaunchRequestHandler(SkillRequest skillRequest)
        {
            var launchRequest = skillRequest.Request as LaunchRequest;

            var speechText = "LaunchIntent";


            //DynamoDB利用の準備
            var userId = skillRequest.Session.User.UserId;
            var tableName = $"{skillName}Table";
            var attrMgr = new AttributesManager(userId, tableName);

            var attr = attrMgr.GetPersistentAttributes();
            var wakeTime = attr?["wakeTime"] ?? "";

            speechText += $"現在設定されている時刻は{wakeTime}です。"+
                "どうしますか？";


            var skillResponse = new SkillResponse
            {
                Version = "1.0",
                Response = new ResponseBody()
            };

            skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech
            {
                Text = speechText
            };
            skillResponse.Response.Reprompt = new Reprompt
            {
                OutputSpeech = new PlainTextOutputSpeech
                {
                    Text = speechText
                }
            };
            skillResponse.Response.Card = new SimpleCard
            {
                Title = "Hello World",
                Content = speechText
            };

            return skillResponse;
        }

        //ご自分で追加したインテントに合わせて名前や処理を変更してください。
        private SkillResponse SetDeadLineIntentHandler(SkillRequest skillRequest)
        {
            var intentRequest = skillRequest.Request as IntentRequest;

            var speechText = "SetDeadLineIntent.";

            var skillResponse = new SkillResponse
            {
                Version = "1.0",
                Response = new ResponseBody()
            };

            var time = intentRequest.Intent.Slots[slot_wakeTime].Value;


            //DynamoDBに追加
            var userId = skillRequest.Session.User.UserId;
            var tableName = $"{skillName}Table";
            var attrMgr=new AttributesManager(userId,tableName);
            attrMgr.SetPersistentAttributes("wakeTime",time);
            //attrMgr.SavePersistentAttributes();
            

            speechText += time;
            skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech
            {
                Text = speechText
            };
            skillResponse.Response.Card = new SimpleCard
            {
                Title = "Hello World",
                Content = speechText
            };
            skillResponse.Response.ShouldEndSession = true;

            return skillResponse;
        }


        private SkillResponse ReportRemainTimeIntentHandler(SkillRequest skillRequest)
        {
            var intentRequest = skillRequest.Request as IntentRequest;

            var speechText = "ReportRemainTimeIntent";

            //現在時刻を取得
            var now = DateTimeOffset.UtcNow;

            //設定時刻をDynamoDBから取得
            var userId = skillRequest.Session.User.UserId;
            var tableName = $"{skillName}Table";
            var attrMgr=new AttributesManager(userId,tableName);
            var attr = attrMgr.GetPersistentAttributes();
            var wakeTimeString = attr?[slot_wakeTime] ?? "";

            //設定時刻をDateTimeOffset型に変換する。
            var wakeTime=DateTimeOffset.Parse()

            var skillResponse = new SkillResponse
            {
                Version = "1.0",
                Response = new ResponseBody()
            };

            skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech
            {
                Text = speechText
            };
            skillResponse.Response.Card = new SimpleCard
            {
                Title = "Hello World",
                Content = speechText
            };
            skillResponse.Response.ShouldEndSession = true;

            return skillResponse;
        }

        /// <summary>
        /// 組み込みインテント用
        /// </summary>
        /// <param name="skillRequest"></param>
        /// <returns></returns>
        private SkillResponse HelpIntentHandler(SkillRequest skillRequest)
        {
            var intentRequest = skillRequest.Request as IntentRequest;

            var speechText = "You can say hello to me!";

            var skillResponse = new SkillResponse
            {
                Version = "1.0",
                Response = new ResponseBody()
            };

            skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech
            {
                Text = speechText
            };
            skillResponse.Response.Reprompt = new Reprompt
            {
                OutputSpeech = new PlainTextOutputSpeech
                {
                    Text = speechText
                }
            };
            skillResponse.Response.Card = new SimpleCard
            {
                Title = "Hello World",
                Content = speechText
            };

            return skillResponse;
        }


        /// <summary>
        /// 組み込みインテント用
        /// </summary>
        /// <param name="skillRequest"></param>
        /// <returns></returns>
        private SkillResponse CancelAndStopIntentHandler(SkillRequest skillRequest)
        {
            var intentRequest = skillRequest.Request as IntentRequest;

            var speechText = "Goodbye!";

            var skillResponse = new SkillResponse
            {
                Version = "1.0",
                Response = new ResponseBody()
            };

            skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech
            {
                Text = speechText
            };
            skillResponse.Response.Card = new SimpleCard
            {
                Title = "Hello World",
                Content = speechText
            };
            skillResponse.Response.ShouldEndSession = true;

            return skillResponse;
        }

        /// <summary>
        /// 組み込みインテント用
        /// </summary>
        /// <param name="skillRequest"></param>
        /// <returns></returns>
        private SkillResponse SessionEndedRequestHandler(SkillRequest skillRequest)
        {
            var sesstionEndedRequest = skillRequest.Request as SessionEndedRequest;

            var skillResponse = new SkillResponse
            {
                Version = "1.0",
                Response = new ResponseBody()
            };

            skillResponse.Response.ShouldEndSession = true;

            return skillResponse;
        }

        /// <summary>
        /// 組み込みインテント用
        /// </summary>
        /// <param name="skillRequest"></param>
        /// <returns></returns>
        private SkillResponse ErrorHandler(SkillRequest skillRequest)
        {
            var speechText = "Sorry, I can't understand the command. Please say again.";

            var skillResponse = new SkillResponse
            {
                Version = "1.0",
                Response = new ResponseBody()
            };

            skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech
            {
                Text = speechText
            };
            skillResponse.Response.Reprompt = new Reprompt
            {
                OutputSpeech = new PlainTextOutputSpeech
                {
                    Text = speechText
                }
            };
            skillResponse.Response.ShouldEndSession = true;//エラーなのでセッションを終了させなければならない。

            return skillResponse;
        }

        #endregion

    }
}
