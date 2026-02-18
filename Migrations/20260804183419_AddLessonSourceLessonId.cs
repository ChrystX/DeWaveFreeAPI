using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeWaveFreeAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonSourceLessonId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_course_instructors_instructor_types_instructor_type_id",
                table: "course_instructors");

            migrationBuilder.DropIndex(
                name: "IX_course_instructors_instructor_type_id",
                table: "course_instructors");

            migrationBuilder.DropColumn(
                name: "instructor_type_id",
                table: "course_instructors");

            migrationBuilder.AlterColumn<DateTime>(
                name: "completed_at",
                table: "student_lesson_progress",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_accessed_at",
                table: "student_lesson_progress",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "max_score",
                table: "student_lesson_progress",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "score",
                table: "student_lesson_progress",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "started_at",
                table: "student_lesson_progress",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "student_lesson_progress",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "certificate_issued_at",
                table: "student_courses",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "completed_at",
                table: "student_courses",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_accessed_at",
                table: "student_courses",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "progress_percent",
                table: "student_courses",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "lesson_type",
                table: "lessons",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "settings_json",
                table: "lessons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "content_object_id",
                table: "lesson_blocks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "forked_from_content_object_id",
                table: "lesson_blocks",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "instructors",
                type: "varchar(255)",
                unicode: false,
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldUnicode: false,
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "instructors",
                type: "datetime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "course_details",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<bool>(
                name: "is_composite",
                table: "block_types",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "content_objects",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    block_type_id = table.Column<int>(type: "int", nullable: false),
                    data_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    h5p_library = table.Column<string>(type: "varchar(127)", unicode: false, maxLength: 127, nullable: true),
                    parent_id = table.Column<int>(type: "int", nullable: true),
                    is_draft = table.Column<bool>(type: "bit", nullable: false),
                    h5p_embed_type = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__content___3213E83F38580BB9", x => x.id);
                    table.ForeignKey(
                        name: "FK_content_objects_block_types",
                        column: x => x.block_type_id,
                        principalTable: "block_types",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_content_objects_content_objects_parent_id",
                        column: x => x.parent_id,
                        principalTable: "content_objects",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "quiz_attempts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    attempt_number = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "in_progress"),
                    answers_json = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    score = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    passed = table.Column<bool>(type: "bit", nullable: true),
                    started_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(getdate())"),
                    submitted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    lesson_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__quiz_att__3213E83FA35578F7", x => x.id);
                    table.ForeignKey(
                        name: "FK_qa_lesson",
                        column: x => x.lesson_id,
                        principalTable: "lessons",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "h5p_content_user_data",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    content_id = table.Column<int>(type: "int", nullable: false),
                    sub_content_id = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "0"),
                    data_id = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    preload = table.Column<bool>(type: "bit", nullable: false),
                    invalidate = table.Column<bool>(type: "bit", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_h5p_content_user_data", x => x.id);
                    table.ForeignKey(
                        name: "FK_h5p_content_user_data_content",
                        column: x => x.content_id,
                        principalTable: "content_objects",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "quiz_attempt_answers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    attempt_id = table.Column<int>(type: "int", nullable: false),
                    question_id = table.Column<int>(type: "int", nullable: false),
                    question_type = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    user_answer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    is_correct = table.Column<bool>(type: "bit", nullable: true),
                    points_earned = table.Column<int>(type: "int", nullable: false),
                    points_possible = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quiz_attempt_answers", x => x.id);
                    table.ForeignKey(
                        name: "FK_quiz_attempt_answers_quiz_attempts_attempt_id",
                        column: x => x.attempt_id,
                        principalTable: "quiz_attempts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_lesson_blocks_content_object_id",
                table: "lesson_blocks",
                column: "content_object_id");

            migrationBuilder.CreateIndex(
                name: "IX_lesson_blocks_forked_from_content_object_id",
                table: "lesson_blocks",
                column: "forked_from_content_object_id");

            migrationBuilder.CreateIndex(
                name: "idx_content_objects_block_type",
                table: "content_objects",
                column: "block_type_id");

            migrationBuilder.CreateIndex(
                name: "idx_content_objects_title",
                table: "content_objects",
                column: "title");

            migrationBuilder.CreateIndex(
                name: "IX_content_objects_parent_id",
                table: "content_objects",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_h5p_content_user_data_content_id",
                table: "h5p_content_user_data",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "UQ_h5p_user_content",
                table: "h5p_content_user_data",
                columns: new[] { "user_id", "content_id", "sub_content_id", "data_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_qaa_attempt",
                table: "quiz_attempt_answers",
                column: "attempt_id");

            migrationBuilder.CreateIndex(
                name: "IX_qaa_question",
                table: "quiz_attempt_answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_qa_lesson",
                table: "quiz_attempts",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "IX_qa_user",
                table: "quiz_attempts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ_qa_attempt",
                table: "quiz_attempts",
                columns: new[] { "user_id", "lesson_id", "attempt_number" },
                unique: true,
                filter: "[lesson_id] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_lesson_blocks_content_objects_content_object_id",
                table: "lesson_blocks",
                column: "content_object_id",
                principalTable: "content_objects",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_lesson_blocks_content_objects_forked_from_content_object_id",
                table: "lesson_blocks",
                column: "forked_from_content_object_id",
                principalTable: "content_objects",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lesson_blocks_content_objects_content_object_id",
                table: "lesson_blocks");

            migrationBuilder.DropForeignKey(
                name: "FK_lesson_blocks_content_objects_forked_from_content_object_id",
                table: "lesson_blocks");

            migrationBuilder.DropTable(
                name: "h5p_content_user_data");

            migrationBuilder.DropTable(
                name: "quiz_attempt_answers");

            migrationBuilder.DropTable(
                name: "content_objects");

            migrationBuilder.DropTable(
                name: "quiz_attempts");

            migrationBuilder.DropIndex(
                name: "IX_lesson_blocks_content_object_id",
                table: "lesson_blocks");

            migrationBuilder.DropIndex(
                name: "IX_lesson_blocks_forked_from_content_object_id",
                table: "lesson_blocks");

            migrationBuilder.DropColumn(
                name: "last_accessed_at",
                table: "student_lesson_progress");

            migrationBuilder.DropColumn(
                name: "max_score",
                table: "student_lesson_progress");

            migrationBuilder.DropColumn(
                name: "score",
                table: "student_lesson_progress");

            migrationBuilder.DropColumn(
                name: "started_at",
                table: "student_lesson_progress");

            migrationBuilder.DropColumn(
                name: "status",
                table: "student_lesson_progress");

            migrationBuilder.DropColumn(
                name: "certificate_issued_at",
                table: "student_courses");

            migrationBuilder.DropColumn(
                name: "completed_at",
                table: "student_courses");

            migrationBuilder.DropColumn(
                name: "last_accessed_at",
                table: "student_courses");

            migrationBuilder.DropColumn(
                name: "progress_percent",
                table: "student_courses");

            migrationBuilder.DropColumn(
                name: "lesson_type",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "settings_json",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "content_object_id",
                table: "lesson_blocks");

            migrationBuilder.DropColumn(
                name: "forked_from_content_object_id",
                table: "lesson_blocks");

            migrationBuilder.DropColumn(
                name: "is_composite",
                table: "block_types");

            migrationBuilder.AlterColumn<DateTime>(
                name: "completed_at",
                table: "student_lesson_progress",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "instructors",
                type: "varchar(255)",
                unicode: false,
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldUnicode: false,
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "instructors",
                type: "datetime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "instructor_type_id",
                table: "course_instructors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "course_details",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.CreateIndex(
                name: "IX_course_instructors_instructor_type_id",
                table: "course_instructors",
                column: "instructor_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_course_instructors_instructor_types_instructor_type_id",
                table: "course_instructors",
                column: "instructor_type_id",
                principalTable: "instructor_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
