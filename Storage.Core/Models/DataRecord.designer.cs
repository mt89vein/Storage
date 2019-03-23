// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: Storage.Core/Protocols/DataRecord.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Storage.Core.Models {

  /// <summary>Holder for reflection information generated from Storage.Core/Protocols/DataRecord.proto</summary>
  public static partial class DataRecordReflection {

    #region Descriptor
    /// <summary>File descriptor for Storage.Core/Protocols/DataRecord.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static DataRecordReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CidTdG9yYWdlLkNvcmUvUHJvdG9jb2xzL0RhdGFSZWNvcmQucHJvdG8SE1N0",
            "b3JhZ2UuQ29yZS5Nb2RlbHMiUQoKRGF0YVJlY29yZBI1CgZoZWFkZXIYASAB",
            "KAsyJS5TdG9yYWdlLkNvcmUuTW9kZWxzLkRhdGFSZWNvcmRIZWFkZXISDAoE",
            "Ym9keRgCIAEoDCIuChBEYXRhUmVjb3JkSGVhZGVyEgoKAmlkGAEgASgDEg4K",
            "Bmxlbmd0aBgCIAEoBWIGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Storage.Core.Models.DataRecord), global::Storage.Core.Models.DataRecord.Parser, new[]{ "PageId", "Body" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Storage.Core.Models.DataRecordHeader), global::Storage.Core.Models.DataRecordHeader.Parser, new[]{ "Id", "Length" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class DataRecord : pb::IMessage<DataRecord> {
    private static readonly pb::MessageParser<DataRecord> _parser = new pb::MessageParser<DataRecord>(() => new DataRecord());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<DataRecord> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Storage.Core.Models.DataRecordReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public DataRecord() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public DataRecord(DataRecord other) : this() {
      header_ = other.header_ != null ? other.header_.Clone() : null;
      body_ = other.body_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public DataRecord Clone() {
      return new DataRecord(this);
    }

    /// <summary>Field number for the "header" field.</summary>
    public const int HeaderFieldNumber = 1;
    private global::Storage.Core.Models.DataRecordHeader header_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Storage.Core.Models.DataRecordHeader Header {
      get { return header_; }
      set {
        header_ = value;
      }
    }

    /// <summary>Field number for the "body" field.</summary>
    public const int BodyFieldNumber = 2;
    private pb::ByteString body_ = pb::ByteString.Empty;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pb::ByteString Body {
      get { return body_; }
      set {
        body_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as DataRecord);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(DataRecord other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Header, other.Header)) return false;
      if (Body != other.Body) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (header_ != null) hash ^= Header.GetHashCode();
      if (Body.Length != 0) hash ^= Body.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (header_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Header);
      }
      if (Body.Length != 0) {
        output.WriteRawTag(18);
        output.WriteBytes(Body);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (header_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Header);
      }
      if (Body.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeBytesSize(Body);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(DataRecord other) {
      if (other == null) {
        return;
      }
      if (other.header_ != null) {
        if (header_ == null) {
          Header = new global::Storage.Core.Models.DataRecordHeader();
        }
        Header.MergeFrom(other.Header);
      }
      if (other.Body.Length != 0) {
        Body = other.Body;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            if (header_ == null) {
              Header = new global::Storage.Core.Models.DataRecordHeader();
            }
            input.ReadMessage(Header);
            break;
          }
          case 18: {
            Body = input.ReadBytes();
            break;
          }
        }
      }
    }

  }

  public sealed partial class DataRecordHeader : pb::IMessage<DataRecordHeader> {
    private static readonly pb::MessageParser<DataRecordHeader> _parser = new pb::MessageParser<DataRecordHeader>(() => new DataRecordHeader());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<DataRecordHeader> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Storage.Core.Models.DataRecordReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public DataRecordHeader() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public DataRecordHeader(DataRecordHeader other) : this() {
      id_ = other.id_;
      length_ = other.length_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public DataRecordHeader Clone() {
      return new DataRecordHeader(this);
    }

    /// <summary>Field number for the "id" field.</summary>
    public const int IdFieldNumber = 1;
    private long id_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public long Id {
      get { return id_; }
      set {
        id_ = value;
      }
    }

    /// <summary>Field number for the "length" field.</summary>
    public const int LengthFieldNumber = 2;
    private int length_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Length {
      get { return length_; }
      set {
        length_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as DataRecordHeader);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(DataRecordHeader other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Id != other.Id) return false;
      if (Length != other.Length) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Id != 0L) hash ^= Id.GetHashCode();
      if (Length != 0) hash ^= Length.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Id != 0L) {
        output.WriteRawTag(8);
        output.WriteInt64(Id);
      }
      if (Length != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(Length);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Id != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(Id);
      }
      if (Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Length);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(DataRecordHeader other) {
      if (other == null) {
        return;
      }
      if (other.Id != 0L) {
        Id = other.Id;
      }
      if (other.Length != 0) {
        Length = other.Length;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            Id = input.ReadInt64();
            break;
          }
          case 16: {
            Length = input.ReadInt32();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code