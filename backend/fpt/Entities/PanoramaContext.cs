using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace fpt.Entites;

public partial class PanoramaContext : DbContext
{
    public PanoramaContext()
    {
    }

    public PanoramaContext(DbContextOptions<PanoramaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Audio> Audios { get; set; }

    public virtual DbSet<Hotspot> Hotspots { get; set; }

    public virtual DbSet<Scene> Scenes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=GS-VinhTT25;Initial Catalog=panorama;Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Audio>(entity =>
        {
            entity.HasKey(e => e.AudioId).HasName("PK__Audio__A28A94506DFC36D6");

            entity.ToTable("Audio");

            entity.Property(e => e.AudioFile).HasMaxLength(255);
            entity.Property(e => e.Language).HasMaxLength(10);
            entity.Property(e => e.SceneId).HasMaxLength(50);

            entity.HasOne(d => d.Scene).WithMany(p => p.Audios)
                .HasForeignKey(d => d.SceneId)
                .HasConstraintName("FK__Audio__SceneId__3D5E1FD2");
        });

        modelBuilder.Entity<Hotspot>(entity =>
        {
            entity.HasKey(e => e.HotspotId).HasName("PK__Hotspot__12EAE163923185F5");

            entity.ToTable("Hotspot");

            entity.Property(e => e.Height).HasMaxLength(50);
            entity.Property(e => e.HotspotIdentifier).HasMaxLength(100);
            entity.Property(e => e.Image).HasMaxLength(255);
            entity.Property(e => e.SceneId).HasMaxLength(50);
            entity.Property(e => e.SceneIdTarget)
                .HasMaxLength(50)
                .HasColumnName("SceneId_Target");
            entity.Property(e => e.Target).HasMaxLength(255);
            entity.Property(e => e.Text).HasMaxLength(255);
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.Url)
                .HasMaxLength(255)
                .HasColumnName("URL");
            entity.Property(e => e.Width).HasMaxLength(50);

            entity.HasOne(d => d.Scene).WithMany(p => p.Hotspots)
                .HasForeignKey(d => d.SceneId)
                .HasConstraintName("FK__Hotspot__SceneId__398D8EEE");
        });

        modelBuilder.Entity<Scene>(entity =>
        {
            entity.HasKey(e => e.SceneId).HasName("PK__Scene__B7A6EE91144646CF");

            entity.ToTable("Scene");

            entity.Property(e => e.SceneId).HasMaxLength(50);
            entity.Property(e => e.AudioEn)
                .HasMaxLength(255)
                .HasColumnName("AudioEN");
            entity.Property(e => e.AudioVn)
                .HasMaxLength(255)
                .HasColumnName("AudioVN");
            entity.Property(e => e.Hfov).HasColumnName("HFov");
            entity.Property(e => e.Panorama).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.Type).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
