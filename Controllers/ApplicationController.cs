using DeWaveFreeAPI.Data;
using DeWaveFreeAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using IOFile = System.IO.File;
using System.Net;
using System.Net.Mail;  

namespace DeWaveFreeAPI.Controllers // Replace with your actual namespace
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin")]
    public class ApplicationController : ControllerBase
    {
        private readonly DeWaveAPIDbContext _context; // Replace with your actual DbContext name
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public ApplicationController(
            DeWaveAPIDbContext context,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _context = context;
            _configuration = configuration;
            _environment = environment;
        }

        [HttpPost("submit")]
        [AllowAnonymous]
        public async Task<IActionResult> SubmitApplication([FromForm] CourseApplicationRequest request)
        {
            try
            {
                // Basic validation
                if (string.IsNullOrWhiteSpace(request.Name) ||
                    string.IsNullOrWhiteSpace(request.Phone) ||
                    string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Address) ||
                    string.IsNullOrWhiteSpace(request.Course))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Please fill in all required fields."
                    });
                }

                // Create new application entity
                var application = new CourseApplication
                {
                    Name = request.Name.Trim(),
                    Phone = request.Phone.Trim(),
                    Email = request.Email.Trim().ToLower(),
                    Address = request.Address.Trim(),
                    Course = request.Course,
                    SubmittedAt = DateTime.UtcNow,
                    Status = "Pending"
                };

                // Handle file upload if present
                if (request.CV != null && request.CV.Length > 0)
                {
                    try
                    {
                        // Validate file size (5MB limit)
                        if (request.CV.Length > 5 * 1024 * 1024)
                        {
                            return BadRequest(new
                            {
                                success = false,
                                message = "File size must be less than 5MB."
                            });
                        }

                        // Validate file extension
                        var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                        var fileExtension = Path.GetExtension(request.CV.FileName).ToLowerInvariant();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            return BadRequest(new
                            {
                                success = false,
                                message = "Only PDF, DOC, and DOCX files are allowed."
                            });
                        }

                        // Create uploads directory if it doesn't exist
                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "cvs");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Generate unique filename
                        var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{fileExtension}";
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        // Save file
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await request.CV.CopyToAsync(fileStream);
                        }

                        application.CvfileName = fileName; // Use exact property name from scaffolded model
                        application.CvfilePath = filePath;
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, new { success = false, message = $"File upload error: {ex.Message}" });
                    }
                }

                // Save to database
                _context.CourseApplications.Add(application);
                await _context.SaveChangesAsync();

                // Send email notification (non-blocking)
                Task.Run(async () =>
                {
                    try
                    {
                        await SendEmailNotification(application);
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't fail the request
                        Console.WriteLine($"Failed to send email: {ex.Message}");
                    }
                });

                return Ok(new
                {
                    success = true,
                    message = "Application submitted successfully! We'll contact you soon.",
                    applicationId = application.Id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error submitting application: {ex.Message}");
                Console.WriteLine("WebRootPath: " + _environment.WebRootPath);
                return StatusCode(500, new
                {
                    success = false,
                    message = "We're experiencing technical difficulties. Please try again later."
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetApplications()
        {
            try
            {
                var applications = await _context.CourseApplications
                    .OrderByDescending(a => a.SubmittedAt)
                    .Select(a => new
                    {
                        a.Id,
                        a.Name,
                        a.Email,
                        a.Phone,
                        a.Course,
                        a.Status,
                        a.SubmittedAt,
                        HasCV = !string.IsNullOrEmpty(a.CvfileName)
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = applications
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving applications: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to retrieve applications."
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetApplication(int id)
        {
            try
            {
                var application = await _context.CourseApplications
                    .Where(a => a.Id == id)
                    .Select(a => new
                    {
                        a.Id,
                        a.Name,
                        a.Email,
                        a.Phone,
                        a.Address,
                        a.Course,
                        a.Status,
                        a.SubmittedAt,
                        CVFileName = a.CvfileName,
                        HasCV = !string.IsNullOrEmpty(a.CvfileName)
                    })
                    .FirstOrDefaultAsync();

                if (application == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Application not found."
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = application
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving application {id}: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to retrieve application."
                });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateApplicationStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Status))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Status is required."
                    });
                }

                var application = await _context.CourseApplications
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (application == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Application not found."
                    });
                }

                var oldStatus = application.Status;
                application.Status = request.Status.Trim();

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Status updated successfully",
                    oldStatus = oldStatus,
                    newStatus = application.Status
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating status for application {id}: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to update status."
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApplication(int id)
        {
            try
            {
                var application = await _context.CourseApplications
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (application == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Application not found."
                    });
                }

                // Delete CV file if it exists
                if (!string.IsNullOrEmpty(application.CvfilePath) && IOFile.Exists(application.CvfilePath))
                {
                    try
                    {
                        IOFile.Delete(application.CvfilePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to delete CV file: {ex.Message}");
                    }
                }

                _context.CourseApplications.Remove(application);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Application deleted successfully."
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting application {id}: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to delete application."
                });
            }
        }

        [HttpGet("{id}/download-cv")]
        public async Task<IActionResult> DownloadCV(int id)
        {
            try
            {
                var application = await _context.CourseApplications
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (application == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Application not found."
                    });
                }

                if (string.IsNullOrEmpty(application.CvfilePath) || !IOFile.Exists(application.CvfilePath))
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "CV file not found."
                    });
                }

                var fileBytes = await IOFile.ReadAllBytesAsync(application.CvfilePath);
                var contentType = GetContentType(application.CvfileName);

                return File(fileBytes, contentType, application.CvfileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading CV for application {id}: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to download CV."
                });
            }
        }

        private async Task SendEmailNotification(CourseApplication application)
        {
            try
            {
                var smtpServer = _configuration["Email:SmtpServer"];
                var port = _configuration["Email:Port"];
                var username = _configuration["Email:Username"];
                var password = _configuration["Email:Password"];
                var fromAddress = _configuration["Email:FromAddress"];
                var adminEmail = _configuration["Email:AdminEmail"];

                // Check if email configuration exists
                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(adminEmail))
                {
                    Console.WriteLine("Email configuration missing. Skipping email notification.");
                    return;
                }

                using var smtpClient = new SmtpClient(smtpServer)
                {
                    Port = int.Parse(port ?? "587"),
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true,
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromAddress ?? username, "Beauty College Applications"),
                    Subject = $"New Course Application - {application.Name} (#{application.Id})",
                    Body = CreateEmailBody(application),
                    IsBodyHtml = true
                };

                mailMessage.To.Add(adminEmail);

                // Attach CV if present
                if (!string.IsNullOrEmpty(application.CvfilePath) && IOFile.Exists(application.CvfilePath))
                {
                    mailMessage.Attachments.Add(new Attachment(application.CvfilePath));
                }

                await smtpClient.SendMailAsync(mailMessage);

                Console.WriteLine($"Email notification sent for application {application.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
                throw; // Re-throw to be caught by the calling method
            }
        }

        private string CreateEmailBody(CourseApplication application)
        {
            return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='utf-8'>
            <title>New Course Application</title>
        </head>
        <body style='font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, ""Helvetica Neue"", Arial, sans-serif; margin: 0; padding: 0; background-color: #fafafa;'>
            <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff;'>
                
                <!-- Header with Logo -->
                <div style='background-color: #ffffff; padding: 40px 40px 30px 40px; border-bottom: 1px solid #f0f0f0;'>
                    <img src='https://i.imgur.com/Qer5Vy8.png' alt='Logo' style='height: 50px; display: block;' />
                </div>
                
                <!-- Title Section -->
                <div style='padding: 40px 40px 20px 40px;'>
                    <h1 style='margin: 0; font-size: 24px; font-weight: 300; color: #1a1a1a; letter-spacing: -0.5px;'>
                        New Course Application
                    </h1>
                    <p style='margin: 8px 0 0 0; font-size: 14px; color: #666666; font-weight: 300;'>
                        Application received and pending review
                    </p>
                </div>
                
                <!-- Application Details -->
                <div style='padding: 0 40px 40px 40px;'>
                    <div style='border: 1px solid #f0f0f0; border-radius: 8px; overflow: hidden;'>
                        
                        <div style='padding: 20px; border-bottom: 1px solid #f0f0f0;'>
                            <div style='font-size: 11px; font-weight: 500; color: #999999; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 6px;'>Application ID</div>
                            <div style='font-size: 15px; color: #1a1a1a;'>#{application.Id}</div>
                        </div>
                        
                        <div style='padding: 20px; border-bottom: 1px solid #f0f0f0;'>
                            <div style='font-size: 11px; font-weight: 500; color: #999999; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 6px;'>Full Name</div>
                            <div style='font-size: 15px; color: #1a1a1a;'>{application.Name}</div>
                        </div>
                        
                        <div style='padding: 20px; border-bottom: 1px solid #f0f0f0;'>
                            <div style='font-size: 11px; font-weight: 500; color: #999999; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 6px;'>Phone Number</div>
                            <div style='font-size: 15px;'><a href='tel:{application.Phone}' style='color: #ec4899; text-decoration: none;'>{application.Phone}</a></div>
                        </div>
                        
                        <div style='padding: 20px; border-bottom: 1px solid #f0f0f0;'>
                            <div style='font-size: 11px; font-weight: 500; color: #999999; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 6px;'>Email Address</div>
                            <div style='font-size: 15px;'><a href='mailto:{application.Email}' style='color: #ec4899; text-decoration: none;'>{application.Email}</a></div>
                        </div>
                        
                        <div style='padding: 20px; border-bottom: 1px solid #f0f0f0;'>
                            <div style='font-size: 11px; font-weight: 500; color: #999999; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 6px;'>Address</div>
                            <div style='font-size: 15px; color: #1a1a1a;'>{application.Address}</div>
                        </div>
                        
                        <div style='padding: 20px; border-bottom: 1px solid #f0f0f0; background-color: #fdf2f8;'>
                            <div style='font-size: 11px; font-weight: 500; color: #999999; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 6px;'>Desired Course</div>
                            <div style='font-size: 15px; color: #ec4899; font-weight: 500;'>{application.Course}</div>
                        </div>
                        
                        <div style='padding: 20px; border-bottom: 1px solid #f0f0f0;'>
                            <div style='font-size: 11px; font-weight: 500; color: #999999; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 6px;'>CV/Resume</div>
                            <div style='font-size: 15px; color: #1a1a1a;'>{(string.IsNullOrEmpty(application.CvfileName) ? "Not provided" : "✓ Attached")}</div>
                        </div>
                        
                        <div style='padding: 20px; border-bottom: 1px solid #f0f0f0;'>
                            <div style='font-size: 11px; font-weight: 500; color: #999999; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 6px;'>Status</div>
                            <div style='display: inline-block; padding: 4px 12px; background-color: #1a1a1a; color: #ffffff; font-size: 12px; border-radius: 4px; font-weight: 400;'>{application.Status}</div>
                        </div>
                        
                        <div style='padding: 20px;'>
                            <div style='font-size: 11px; font-weight: 500; color: #999999; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 6px;'>Submitted</div>
                            <div style='font-size: 15px; color: #1a1a1a;'>{application.SubmittedAt:dddd, MMMM dd, yyyy 'at' HH:mm} UTC</div>
                        </div>
                        
                    </div>
                    
                    <!-- Next Steps -->
                    <div style='margin-top: 30px; padding: 24px; background-color: #fdf2f8; border-radius: 8px; border-left: 3px solid #ec4899;'>
                        <div style='font-size: 13px; font-weight: 500; color: #1a1a1a; margin-bottom: 12px;'>Next Steps</div>
                        <div style='font-size: 14px; color: #4a4a4a; line-height: 1.6; font-weight: 300;'>
                            • Review the application details<br/>
                            • Contact the student within 24 hours<br/>
                            • Schedule an interview if appropriate<br/>
                            • Update the application status in the system
                        </div>
                    </div>
                </div>
                
                <!-- Footer -->
                <div style='padding: 30px 40px; background-color: #fafafa; border-top: 1px solid #f0f0f0;'>
                    <p style='margin: 0; font-size: 12px; color: #999999; font-weight: 300; line-height: 1.6;'>
                        This email was automatically generated from the course application system.<br/>
                        Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
                    </p>
                </div>
                
            </div>
        </body>
        </html>";
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
        }
    }

    // Request DTOs
    public class CourseApplicationRequest
    {
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public string Course { get; set; } = "";
        public IFormFile? CV { get; set; }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; } = "";
    }
}