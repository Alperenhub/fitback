namespace fitback.Dtos.CodeDtos
{
    public class StudentDto
    {
        public class StudentResponseDto
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public bool IsApprovedByTrainer { get; set; }
            public int? TrainerId { get; set; }

            public int? TrainerCodeId { get; set; }
        }

        // Student onaylama / approve
        public class ApproveStudentDto
        {
            public int StudentId { get; set; }
            public bool Approve { get; set; } // true → onayla, false → reddet
        }
    }
}
