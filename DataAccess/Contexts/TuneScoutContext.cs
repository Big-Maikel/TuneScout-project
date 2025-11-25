using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Logic.Models;

namespace DataAccess.Contexts;

public partial class TuneScoutContext : DbContext
{
    public TuneScoutContext()
    {
    }

    public TuneScoutContext(DbContextOptions<TuneScoutContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Mood> Moods { get; set; }

    public virtual DbSet<Preference> Preferences { get; set; }

    public virtual DbSet<Swipe> Swipes { get; set; }

    public virtual DbSet<Track> Tracks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=localhost;Database=TuneScout;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Genre>(entity =>
        {
            entity.ToTable("genre");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("text")
                .HasColumnName("name");
            entity.Property(e => e.SpotifyUri)
                .HasColumnType("text")
                .HasColumnName("spotify_uri");
        });

        modelBuilder.Entity<Mood>(entity =>
        {
            entity.ToTable("mood");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasColumnType("text")
                .HasColumnName("name");
        });

        modelBuilder.Entity<Preference>(entity =>
        {
            entity.ToTable("preference");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GenreId).HasColumnName("genre_id");
            entity.Property(e => e.LanguageId).HasColumnName("language_id");
            entity.Property(e => e.MoodId).HasColumnName("mood_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Genre).WithMany(p => p.Preferences)
                .HasForeignKey(d => d.GenreId)
                .HasConstraintName("FK_preference_genre");

            entity.HasOne(d => d.Mood).WithMany(p => p.Preferences)
                .HasForeignKey(d => d.MoodId)
                .HasConstraintName("FK_preference_mood");

            entity.HasOne(d => d.User).WithMany(p => p.Preferences)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_preference_user");
        });

        modelBuilder.Entity<Swipe>(entity =>
        {
            entity.ToTable("swipe");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Direction)
                .HasColumnType("text")
                .HasColumnName("direction");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("timestamp");
            entity.Property(e => e.TrackId).HasColumnName("track_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Track).WithMany(p => p.Swipes)
                .HasForeignKey(d => d.TrackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_swipe_track");

            entity.HasOne(d => d.User).WithMany(p => p.Swipes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_swipe_user");
        });

        modelBuilder.Entity<Track>(entity =>
        {
            entity.ToTable("track");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Artist)
                .HasColumnType("text")
                .HasColumnName("artist");
            entity.Property(e => e.Explicit).HasColumnName("explicit");
            entity.Property(e => e.GenreId).HasColumnName("genre_id");
            entity.Property(e => e.LanguageId).HasColumnName("language_id");
            entity.Property(e => e.MoodId).HasColumnName("mood_id");
            entity.Property(e => e.Name)
                .HasColumnType("text")
                .HasColumnName("name");
            entity.Property(e => e.PreviewUrl)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("preview_url");
            entity.Property(e => e.SpotifyUri)
                .HasColumnType("text")
                .HasColumnName("spotify_uri");
            entity.Property(e => e.Valence).HasColumnName("valence");

            entity.HasOne(d => d.Genre).WithMany(p => p.Tracks)
                .HasForeignKey(d => d.GenreId)
                .HasConstraintName("FK_track_genre");

            entity.HasOne(d => d.Mood).WithMany(p => p.Tracks)
                .HasForeignKey(d => d.MoodId)
                .HasConstraintName("FK_track_mood");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("user");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasColumnType("text")
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasColumnType("text")
                .HasColumnName("name");
            entity.Property(e => e.NoExplicit).HasColumnName("no_explicit");
            entity.Property(e => e.Password)
                .HasColumnType("text")
                .HasColumnName("password");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
