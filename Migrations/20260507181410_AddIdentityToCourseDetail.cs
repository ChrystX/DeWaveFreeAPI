using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeWaveFreeAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityToCourseDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "block_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__block_ty__3213E83FDC459AAD", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "blog",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    author_id = table.Column<int>(type: "int", nullable: true),
                    summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    thumbnail_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "draft"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysdatetime())"),
                    published_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    view_count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__blog__3213E83FF4ACE46E", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Category__3214EC0713EA737F", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CourseApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Course = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CVFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CVFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getutcdate())"),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "Pending")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CourseAp__3214EC0783C8197B", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "instructor_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__instruct__3213E83F07A0CDBA", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Roles__3214EC075882ED90", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_sequences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RolePrefix = table.Column<string>(type: "char(2)", unicode: false, fixedLength: true, maxLength: 2, nullable: false),
                    Month = table.Column<byte>(type: "tinyint", nullable: false),
                    Year = table.Column<short>(type: "smallint", nullable: false),
                    LastSequence = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserSequ__3214EC07E4B097EC", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "blog_details",
                columns: table => new
                {
                    blog_id = table.Column<int>(type: "int", nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    seo_title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    seo_description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    seo_keywords = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    tags = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    extra_json = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__blog_det__2975AA2855C60D32", x => x.blog_id);
                    table.ForeignKey(
                        name: "FK_blog_details_blog",
                        column: x => x.blog_id,
                        principalTable: "blog",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "blog_tags",
                columns: table => new
                {
                    blog_id = table.Column<int>(type: "int", nullable: false),
                    tag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__blog_tag__34B4ABE8F7FE4C7E", x => new { x.blog_id, x.tag });
                    table.ForeignKey(
                        name: "FK_blog_tags_blog",
                        column: x => x.blog_id,
                        principalTable: "blog",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    instructor = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    duration = table.Column<int>(type: "int", nullable: true),
                    video_count = table.Column<int>(type: "int", nullable: true),
                    rating = table.Column<decimal>(type: "decimal(3,2)", nullable: true),
                    image = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    instructor_id = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Course_Category",
                        column: x => x.CategoryId,
                        principalTable: "category",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    EmailVerificationToken = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PasswordResetToken = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PasswordResetTokenExpires = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    DisplayId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__3214EC072DC8FA20", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "course_details",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    course_id = table.Column<int>(type: "int", nullable: true),
                    ShortDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FullDescriptionHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToolsRequired = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    hero_image = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__course_d__3213E83F07F62793", x => x.id);
                    table.ForeignKey(
                        name: "FK_course_details_courses",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "course_faqs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    question = table.Column<string>(type: "text", nullable: false),
                    answer = table.Column<string>(type: "text", nullable: true),
                    sort_order = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CourseFa__3214EC07576B2278", x => x.id);
                    table.ForeignKey(
                        name: "FK_CourseFaq_Course",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "course_learning_sections",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__course_l__3213E83F738C8CE9", x => x.id);
                    table.ForeignKey(
                        name: "FK_learning_sections_course",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "course_sections",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    content_html = table.Column<string>(type: "text", nullable: true),
                    video_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    thumbnail_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    duration_minutes = table.Column<int>(type: "int", nullable: true),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CourseSy__3214EC07BFACF63A", x => x.id);
                    table.ForeignKey(
                        name: "FK_CourseSyllabus_Course",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "course_events",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    start_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    event_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "online"),
                    visibility = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "course_only"),
                    track_attendance = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    requires_registration = table.Column<bool>(type: "bit", nullable: false),
                    meeting_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    location = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    thumbnail_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    preview_video_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    capacity = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    created_by_user_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__course_e__3213E83F82862F97", x => x.id);
                    table.ForeignKey(
                        name: "fk_course_events_user",
                        column: x => x.created_by_user_id,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "instructors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    bio = table.Column<string>(type: "text", nullable: true),
                    image_url = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: true),
                    contact_email = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    phone_number = table.Column<string>(type: "char(14)", unicode: false, fixedLength: true, maxLength: 14, nullable: true),
                    certifications = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    headline = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    specialization = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__instruct__3214EC070B3A630E", x => x.Id);
                    table.ForeignKey(
                        name: "FK_instructors_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RefreshT__3214EC07F8A50CB2", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    phone_number = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    emergency_contact = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    emergency_phone = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Students__3214EC07B92067FA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "course_images",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(24)", unicode: false, maxLength: 24, nullable: false),
                    detail_id = table.Column<int>(type: "int", nullable: true),
                    url = table.Column<string>(type: "text", nullable: false),
                    caption = table.Column<string>(type: "text", nullable: true),
                    order = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    is_main_image = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                    uploaded_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__course_i__3213E83F9F132558", x => x.id);
                    table.ForeignKey(
                        name: "FK__course_im__detai__6754599E",
                        column: x => x.detail_id,
                        principalTable: "course_details",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lessons",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    section_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__lessons__3213E83F94DDB14A", x => x.id);
                    table.ForeignKey(
                        name: "FK_lessons_section",
                        column: x => x.section_id,
                        principalTable: "course_learning_sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "course_event_courses",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "int", nullable: false),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__course_e__DB81185DFE7574B4", x => new { x.event_id, x.course_id });
                    table.ForeignKey(
                        name: "fk_cec_course",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "fk_cec_event",
                        column: x => x.event_id,
                        principalTable: "course_events",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "course_instructors",
                columns: table => new
                {
                    course_id = table.Column<int>(type: "int", nullable: false),
                    instructor_id = table.Column<int>(type: "int", nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: true),
                    instructor_type_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_instructors", x => new { x.course_id, x.instructor_id });
                    table.ForeignKey(
                        name: "FK_course_instructors_courses",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_course_instructors_instructor_types_instructor_type_id",
                        column: x => x.instructor_type_id,
                        principalTable: "instructor_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_course_instructors_instructors",
                        column: x => x.instructor_id,
                        principalTable: "instructors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "event_attendance",
                columns: table => new
                {
                    student_id = table.Column<int>(type: "int", nullable: false),
                    event_id = table.Column<int>(type: "int", nullable: false),
                    attended = table.Column<bool>(type: "bit", nullable: false),
                    joined_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "absent")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__event_at__480409E89F7B98DE", x => new { x.student_id, x.event_id });
                    table.ForeignKey(
                        name: "fk_event_attendance_event",
                        column: x => x.event_id,
                        principalTable: "course_events",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_event_attendance_student",
                        column: x => x.student_id,
                        principalTable: "Students",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "event_enrollments",
                columns: table => new
                {
                    student_id = table.Column<int>(type: "int", nullable: false),
                    event_id = table.Column<int>(type: "int", nullable: false),
                    registered_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "registered")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__event_en__480409E874514955", x => new { x.student_id, x.event_id });
                    table.ForeignKey(
                        name: "fk_event_enrollments_event",
                        column: x => x.event_id,
                        principalTable: "course_events",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_event_enrollments_student",
                        column: x => x.student_id,
                        principalTable: "Students",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(9,2)", nullable: false),
                    order_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    snap_token = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Payments__3214EC075C33B68C", x => x.id);
                    table.ForeignKey(
                        name: "FK_Payments_Student",
                        column: x => x.student_id,
                        principalTable: "Students",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_payments_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_courses",
                columns: table => new
                {
                    student_id = table.Column<int>(type: "int", nullable: false),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    enrolled_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_courses", x => new { x.student_id, x.course_id });
                    table.ForeignKey(
                        name: "FK_student_courses_Students_student_id",
                        column: x => x.student_id,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_student_courses_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lesson_blocks",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    lesson_id = table.Column<int>(type: "int", nullable: false),
                    block_type_id = table.Column<int>(type: "int", nullable: false),
                    order_index = table.Column<int>(type: "int", nullable: false),
                    data_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__lesson_b__3213E83FFCC61603", x => x.id);
                    table.ForeignKey(
                        name: "FK_blocks_lesson",
                        column: x => x.lesson_id,
                        principalTable: "lessons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_blocks_type",
                        column: x => x.block_type_id,
                        principalTable: "block_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "student_lesson_progress",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    lesson_id = table.Column<int>(type: "int", nullable: false),
                    completed_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__student___3213E83F5ED50D46", x => x.id);
                    table.ForeignKey(
                        name: "FK_slp_student",
                        column: x => x.student_id,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_student_lesson_progress_lessons_lesson_id",
                        column: x => x.lesson_id,
                        principalTable: "lessons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ__block_ty__72E12F1B3F33E3D0",
                table: "block_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__blog__32DD1E4C9A687D8B",
                table: "blog",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_course_details_course_id",
                table: "course_details",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "idx_cec_course",
                table: "course_event_courses",
                columns: new[] { "course_id", "event_id" });

            migrationBuilder.CreateIndex(
                name: "idx_cec_event",
                table: "course_event_courses",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "idx_course_events_active",
                table: "course_events",
                column: "start_time",
                filter: "([is_active]=(1))");

            migrationBuilder.CreateIndex(
                name: "idx_course_events_created_by",
                table: "course_events",
                columns: new[] { "created_by_user_id", "start_time" });

            migrationBuilder.CreateIndex(
                name: "idx_course_events_visibility_time",
                table: "course_events",
                columns: new[] { "visibility", "start_time" });

            migrationBuilder.CreateIndex(
                name: "IX_course_faqs_course_id",
                table: "course_faqs",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_images_detail_id",
                table: "course_images",
                column: "detail_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_instructors_instructor_id",
                table: "course_instructors",
                column: "instructor_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_instructors_instructor_type_id",
                table: "course_instructors",
                column: "instructor_type_id");

            migrationBuilder.CreateIndex(
                name: "idx_learning_sections_course",
                table: "course_learning_sections",
                columns: new[] { "course_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "IX_course_sections_course_id",
                table: "course_sections",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_courses_CategoryId",
                table: "courses",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "idx_event_attendance_event",
                table: "event_attendance",
                columns: new[] { "event_id", "student_id" });

            migrationBuilder.CreateIndex(
                name: "idx_event_enrollments_event",
                table: "event_enrollments",
                columns: new[] { "event_id", "student_id" });

            migrationBuilder.CreateIndex(
                name: "UQ__instruct__72E12F1B722BA49E",
                table: "instructor_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_instructors_user_id",
                table: "instructors",
                column: "user_id",
                unique: true,
                filter: "[user_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "idx_blocks_lesson",
                table: "lesson_blocks",
                columns: new[] { "lesson_id", "order_index" });

            migrationBuilder.CreateIndex(
                name: "IX_lesson_blocks_block_type_id",
                table: "lesson_blocks",
                column: "block_type_id");

            migrationBuilder.CreateIndex(
                name: "idx_lessons_section",
                table: "lessons",
                columns: new[] { "section_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "IX_payments_course_id",
                table: "payments",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_student_id",
                table: "payments",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "refresh_tokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "refresh_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ__Roles__737584F621FF8B55",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_courses_course_id",
                table: "student_courses",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "idx_slp_lesson_id",
                table: "student_lesson_progress",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "idx_slp_student_id",
                table: "student_lesson_progress",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "UQ_student_lesson",
                table: "student_lesson_progress",
                columns: new[] { "student_id", "lesson_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Students_UserId",
                table: "Students",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_UserSequences",
                table: "user_sequences",
                columns: new[] { "Role", "Month", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__536C85E4FCAACD33",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Users_DisplayId",
                table: "Users",
                column: "DisplayId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Users_Email",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blog_details");

            migrationBuilder.DropTable(
                name: "blog_tags");

            migrationBuilder.DropTable(
                name: "course_event_courses");

            migrationBuilder.DropTable(
                name: "course_faqs");

            migrationBuilder.DropTable(
                name: "course_images");

            migrationBuilder.DropTable(
                name: "course_instructors");

            migrationBuilder.DropTable(
                name: "course_sections");

            migrationBuilder.DropTable(
                name: "CourseApplications");

            migrationBuilder.DropTable(
                name: "event_attendance");

            migrationBuilder.DropTable(
                name: "event_enrollments");

            migrationBuilder.DropTable(
                name: "lesson_blocks");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "student_courses");

            migrationBuilder.DropTable(
                name: "student_lesson_progress");

            migrationBuilder.DropTable(
                name: "user_sequences");

            migrationBuilder.DropTable(
                name: "blog");

            migrationBuilder.DropTable(
                name: "course_details");

            migrationBuilder.DropTable(
                name: "instructor_types");

            migrationBuilder.DropTable(
                name: "instructors");

            migrationBuilder.DropTable(
                name: "course_events");

            migrationBuilder.DropTable(
                name: "block_types");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "lessons");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "course_learning_sections");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "courses");

            migrationBuilder.DropTable(
                name: "category");
        }
    }
}
