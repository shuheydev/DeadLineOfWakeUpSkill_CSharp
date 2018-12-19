using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using AlexaPersistentAttributesManager;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using DeadLineOfWakeUpSkill_CSharp.ExtensionMethods;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace DeadLineOfWakeUpSkill_CSharp
{
    public class Function
    {
        private readonly string skillName = "DeadLineOfWakeUpSkill";
        private readonly string cardTitle = "�������~�b�g";

        private readonly string slot_wakeTimeUserSet = "wakeTimeUserSet";
        private readonly string slot_dayTypeUserSet = "dayTypeUserSet";

        private string db_wakeTimeWorkday = "wakeTimeWorkday";
        private string db_wakeTimeHoliday = "wakeTimeHoliday";


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
                //�^�X�C�b�`�̗��p
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
                            case "ReportSettingIntent":
                                skillResponse = ReportSettingIntentHandler(skillRequest);
                                break;
                            case "HelpAboutReportRemainTimeIntent":
                                skillResponse = HelpAboutReportRemainTimeIntent(skillRequest);
                                break;
                            case "HelpAboutReportSettingIntent":
                                skillResponse = HelpAboutReportSettingIntent(skillRequest);
                                break;
                            case "HelpAboutSetTimeIntent":
                                skillResponse = HelpAboutSetTimeIntent(skillRequest);
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




        #region �e�C���e���g�A���N�G�X�g�ɑΉ����鏈����S�����郁�\�b�h����
        /// <summary>
        /// 
        /// </summary>
        /// <param name="skillRequest"></param>
        /// <returns></returns>
        private SkillResponse LaunchRequestHandler(SkillRequest skillRequest)
        {
            var launchRequest = skillRequest.Request as LaunchRequest;

            var speechText = "";


            //DynamoDB���p�̏���
            var userId = skillRequest.Session.User.UserId;
            var tableName = $"{skillName}Table";
            var attrMgr = new AttributesManager(userId, tableName);

            var dbHelper = new DBHelper(attrMgr);


            var nowJst = DateTimeUtility.GetNowJst();
            var wakeTimeToday = dbHelper.GetTodaysWakeTimeString(nowJst);

            if (!string.IsNullOrEmpty(wakeTimeToday))
            {
                speechText += MessageComposer.ReportDiffTime(dbHelper, nowJst);
            }
            else
            {
                speechText += MessageComposer.ReportSetting(dbHelper);
            }


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
                    Text = MessageComposer.ReportAskAnythingElse()
                }
            };
            skillResponse.Response.Card = new SimpleCard
            {
                Title = cardTitle,
                Content = speechText
            };

            return skillResponse;
        }

        //�������Œǉ������C���e���g�ɍ��킹�Ė��O�⏈����ύX���Ă��������B
        private SkillResponse SetDeadLineIntentHandler(SkillRequest skillRequest)
        {
            var intentRequest = skillRequest.Request as IntentRequest;

            var speechText = "";

            //�X���b�g����l���擾
            var dayTypeInSlot = intentRequest.Intent.Slots.GetOrDefault(slot_dayTypeUserSet)?.Value ?? "����";
            var wakeTimeInSlot = intentRequest.Intent.Slots.GetOrDefault(slot_wakeTimeUserSet)?.Value ?? "";

            var wakeTimeInSlotDateTimeOffset = DateTimeOffset.Parse(wakeTimeInSlot + "+09:00");//���{���Ԃ�

            //DynamoDB�ɒǉ�
            var userId = skillRequest.Session.User.UserId;
            var tableName = $"{skillName}Table";
            var attrMgr = new AttributesManager(userId, tableName);

            var dbHelper = new DBHelper(attrMgr);


            //�������x����
            if (dayTypeInSlot == "����")
            {
                attrMgr.SetPersistentAttributes(db_wakeTimeWorkday, wakeTimeInSlotDateTimeOffset.ToString());
                attrMgr.SetPersistentAttributes(db_wakeTimeHoliday, dbHelper.wakeTimeHolidayString);
            }
            else if (dayTypeInSlot == "�x��")
            {
                attrMgr.SetPersistentAttributes(db_wakeTimeWorkday, dbHelper.wakeTimeWorkdayString);
                attrMgr.SetPersistentAttributes(db_wakeTimeHoliday, wakeTimeInSlotDateTimeOffset.ToString());
            }
            attrMgr.SavePersistentAttributes();



            speechText += MessageComposer.ReportTimeHasSet(dayTypeInSlot, wakeTimeInSlotDateTimeOffset);

            //���X�|���X
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
                Title = skillName,
                Content = speechText
            };
            skillResponse.Response.ShouldEndSession = true;

            return skillResponse;
        }



        private SkillResponse ReportRemainTimeIntentHandler(SkillRequest skillRequest)
        {
            var intentRequest = skillRequest.Request as IntentRequest;

            var speechText = "";

            var nowJst = DateTimeUtility.GetNowJst();

            //�ݒ莞����DynamoDB����擾
            var userId = skillRequest.Session.User.UserId;
            var tableName = $"{skillName}Table";
            var attrMgr = new AttributesManager(userId, tableName);


            var dbHelper = new DBHelper(attrMgr);

            var wakeTimeString = dbHelper.GetTodaysWakeTimeString(nowJst);
            if (!string.IsNullOrEmpty(wakeTimeString))
            {

                speechText += MessageComposer.ReportDiffTime(dbHelper, nowJst);

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
                    Title = skillName,
                    Content = speechText
                };
                skillResponse.Response.ShouldEndSession = true;

                return skillResponse;
            }
            else
            {

                speechText += MessageComposer.ReportNotSetWakeTime(nowJst);

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
                    Title = skillName,
                    Content = speechText
                };
                skillResponse.Response.ShouldEndSession = true;

                return skillResponse;
            }

        }


        private SkillResponse ReportSettingIntentHandler(SkillRequest skillRequest)
        {
            var intentRequest = skillRequest.Request as IntentRequest;

            var speechText = "";

            ////�X���b�g����l���擾
            //var dayTypeInSlot = intentRequest.Intent.Slots.GetOrDefault(slot_dayTypeUserSet)?.Value ?? "����";
            //var wakeTimeInSlot = intentRequest.Intent.Slots.GetOrDefault(slot_wakeTimeUserSet)?.Value ?? "";

            //var wakeTimeInSlotDateTimeOffset = DateTimeOffset.Parse(wakeTimeInSlot + "+09:00");//���{���Ԃ�

            //DynamoDB���p�̏���
            var userId = skillRequest.Session.User.UserId;
            var tableName = $"{skillName}Table";
            var attrMgr = new AttributesManager(userId, tableName);

            var dbHelper = new DBHelper(attrMgr);

            speechText += MessageComposer.ReportSetting(dbHelper);



            //���X�|���X
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
                Title = skillName,
                Content = speechText
            };
            skillResponse.Response.ShouldEndSession = true;

            return skillResponse;
        }

        /// <summary>
        /// �g�ݍ��݃C���e���g�p
        /// </summary>
        /// <param name="skillRequest"></param>
        /// <returns></returns>
        private SkillResponse HelpIntentHandler(SkillRequest skillRequest)
        {
            var intentRequest = skillRequest.Request as IntentRequest;


            var speechText = "";


            speechText += MessageComposer.ReportHelpMenu();


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
                    Text = MessageComposer.ReportAskAnythingElse()
                }
            };
            skillResponse.Response.Card = new SimpleCard
            {
                Title = skillName,
                Content = speechText
            };

            return skillResponse;
        }


        /// <summary>
        /// �g�ݍ��݃C���e���g�p
        /// </summary>
        /// <param name="skillRequest"></param>
        /// <returns></returns>
        private SkillResponse HelpAboutReportRemainTimeIntent(SkillRequest skillRequest)
        {
            var intentRequest = skillRequest.Request as IntentRequest;


            var speechText = "";


            speechText += MessageComposer.ReportHelpAboutReportRemainTime();
            speechText += MessageComposer.ReportAskAnythingElse();

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
                    Text = MessageComposer.ReportAskAnythingElse()
                }
            };
            skillResponse.Response.Card = new SimpleCard
            {
                Title = skillName,
                Content = speechText
            };

            return skillResponse;
        }


        /// <summary>
        /// �g�ݍ��݃C���e���g�p
        /// </summary>
        /// <param name="skillRequest"></param>
        /// <returns></returns>
        private SkillResponse HelpAboutReportSettingIntent(SkillRequest skillRequest)
        {
            var intentRequest = skillRequest.Request as IntentRequest;


            var speechText = "";


            speechText += MessageComposer.ReportHelpAboutReportSetting();
            speechText += MessageComposer.ReportAskAnythingElse();

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
                    Text = MessageComposer.ReportAskAnythingElse()
                }
            };
            skillResponse.Response.Card = new SimpleCard
            {
                Title = skillName,
                Content = speechText
            };

            return skillResponse;
        }


        /// <summary>
        /// �g�ݍ��݃C���e���g�p
        /// </summary>
        /// <param name="skillRequest"></param>
        /// <returns></returns>
        private SkillResponse HelpAboutSetTimeIntent(SkillRequest skillRequest)
        {
            var intentRequest = skillRequest.Request as IntentRequest;


            var speechText = "";


            speechText += MessageComposer.ReportHelpAboutSetTime();

            speechText += MessageComposer.ReportAskAnythingElse();

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
                    Text = MessageComposer.ReportAskAnythingElse()
                }
            };
            skillResponse.Response.Card = new SimpleCard
            {
                Title = skillName,
                Content = speechText
            };

            return skillResponse;
        }


        /// <summary>
        /// �g�ݍ��݃C���e���g�p
        /// </summary>
        /// <param name="skillRequest"></param>
        /// <returns></returns>
        private SkillResponse CancelAndStopIntentHandler(SkillRequest skillRequest)
        {
            var intentRequest = skillRequest.Request as IntentRequest;

            var speechText = MessageComposer.ReportGoodby();

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
                Title = cardTitle,
                Content = speechText
            };
            skillResponse.Response.ShouldEndSession = true;

            return skillResponse;
        }

        /// <summary>
        /// �g�ݍ��݃C���e���g�p
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
        /// �g�ݍ��݃C���e���g�p
        /// </summary>
        /// <param name="skillRequest"></param>
        /// <returns></returns>
        private SkillResponse ErrorHandler(SkillRequest skillRequest)
        {
            var speechText = "���߂�Ȃ����B�悭�������܂���ł����B������x�����Ă��������B";

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
            skillResponse.Response.ShouldEndSession = true;//�G���[�Ȃ̂ŃZ�b�V�������I�������Ȃ���΂Ȃ�Ȃ��B

            return skillResponse;
        }

        #endregion

    }
}
