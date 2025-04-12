public class  EmailSendDto {

    public String To { get; set; }

    public String Subject { get; set; }

    public String Body { get; set; }

    public EmailSendDto(string To, string Subject, string Body)
    {
        this.To = To;
        this.Subject = Subject;
        this.Body = Body;
    }

    
}