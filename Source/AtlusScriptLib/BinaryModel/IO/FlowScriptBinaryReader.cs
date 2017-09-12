﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using AtlusScriptLib.IO;

namespace AtlusScriptLib.BinaryModel.IO
{
    public sealed class FlowScriptBinaryReader : IDisposable
    {
        private bool mDisposed;
        private long mPositionBase;
        private EndianBinaryReader mReader;
        private FlowScriptBinaryFormatVersion mVersion;

        public FlowScriptBinaryReader( Stream stream, FlowScriptBinaryFormatVersion version, bool leaveOpen = false )
        {
            mPositionBase = stream.Position;
            mReader = new EndianBinaryReader( stream, Encoding.Default, leaveOpen, version.HasFlag( FlowScriptBinaryFormatVersion.BigEndian ) ? Endianness.BigEndian : Endianness.LittleEndian );
            mVersion = version;
        }

        public FlowScriptBinary ReadBinary()
        {
            FlowScriptBinary instance = new FlowScriptBinary()
            {
                mHeader = ReadHeader()
            };

            instance.mSectionHeaders = ReadSectionHeaders( ref instance.mHeader );

            for ( int i = 0; i < instance.mSectionHeaders.Length; i++ )
            {
                ref var sectionHeader = ref instance.mSectionHeaders[i];

                switch ( sectionHeader.SectionType )
                {
                    case FlowScriptBinarySectionType.ProcedureLabelSection:
                        instance.mProcedureLabelSection = ReadLabelSection( ref sectionHeader );
                        break;

                    case FlowScriptBinarySectionType.JumpLabelSection:
                        instance.mJumpLabelSection = ReadLabelSection( ref sectionHeader );
                        break;

                    case FlowScriptBinarySectionType.TextSection:
                        instance.mTextSection = ReadTextSection( ref sectionHeader );
                        break;

                    case FlowScriptBinarySectionType.MessageScriptSection:
                        instance.mMessageScriptSection = ReadMessageScriptSection( ref sectionHeader );
                        break;

                    case FlowScriptBinarySectionType.StringSection:

                        // fix for early, broken files
                        // see: nocturne e500.bf
                        if ( sectionHeader.FirstElementAddress == instance.mHeader.FileSize )
                        {
                            instance.mHeader.FileSize = ( int )( mReader.BaseStreamLength - mPositionBase );
                            sectionHeader.ElementCount = instance.mHeader.FileSize - sectionHeader.FirstElementAddress;
                        }

                        instance.mStringSection = ReadStringSection( ref sectionHeader );
                        break;

                    default:
                        throw new InvalidDataException( "Unknown section type" );
                }
            }

            instance.mFormatVersion = GetDetectedFormatVersion();

            return instance;
        }

        public FlowScriptBinaryHeader ReadHeader()
        {
            ReadHeaderInternal( out FlowScriptBinaryHeader header );
            MaybeSwapHeaderEndianness( ref header );

            return header;
        }

        public FlowScriptBinarySectionHeader[] ReadSectionHeaders( ref FlowScriptBinaryHeader header )
        {
            return mReader.ReadStruct<FlowScriptBinarySectionHeader>( header.SectionCount );
        }

        public FlowScriptBinaryLabel[] ReadLabelSection( ref FlowScriptBinarySectionHeader sectionHeader )
        {
            EnsureSectionHeaderInitialValidState( ref sectionHeader );

            if ( sectionHeader.ElementSize != FlowScriptBinaryLabel.SIZE_V1 &&
                sectionHeader.ElementSize != FlowScriptBinaryLabel.SIZE_V2 &&
                sectionHeader.ElementSize != FlowScriptBinaryLabel.SIZE_V3 )
            {
                throw new InvalidDataException( "Unknown size for label" );
            }

            MaybeSwapVersionEndiannessByLabelSectionHeader( ref sectionHeader );

            var labels = new FlowScriptBinaryLabel[sectionHeader.ElementCount];

            for ( int i = 0; i < labels.Length; i++ )
            {
                // length of string is equal to the size of the label without the 2 Int32 fields
                int nameStringLength = sectionHeader.ElementSize - ( sizeof( int ) * 2 );

                var label = new FlowScriptBinaryLabel()
                {
                    Name = mReader.ReadString( StringBinaryFormat.FixedLength, nameStringLength ),
                    InstructionIndex = mReader.ReadInt32(),
                    Reserved = mReader.ReadInt32()
                };

                // Would indicate a possible endianness issue
                if ( label.InstructionIndex >= int.MaxValue )
                {
                    throw new InvalidDataException( "Invalid label offset" );
                }

                // Should be zero
                if ( label.Reserved != 0 )
                {
                    throw new InvalidDataException( "Label reserved field isn't 0" );
                }

                labels[i] = label;
            }

            return labels;
        }

        public FlowScriptBinaryInstruction[] ReadTextSection( ref FlowScriptBinarySectionHeader sectionHeader )
        {
            EnsureSectionHeaderInitialValidState( ref sectionHeader );

            if ( sectionHeader.ElementSize != FlowScriptBinaryInstruction.SIZE )
            {
                throw new InvalidDataException( $"{FlowScriptBinarySectionType.TextSection} unit size must be 4" );
            }

            var instructions = new FlowScriptBinaryInstruction[sectionHeader.ElementCount];
            for ( int i = 0; i < instructions.Length; i++ )
            {
                FlowScriptBinaryInstruction instruction = new FlowScriptBinaryInstruction();

                if ( i != 0 && instructions[i - 1].Opcode == FlowScriptOpcode.PUSHI )
                {
                    instruction.Opcode = unchecked(( FlowScriptOpcode )( -1 ));
                    instruction.OperandInt = mReader.ReadInt32();
                }
                else if ( i != 0 && instructions[i - 1].Opcode == FlowScriptOpcode.PUSHF )
                {
                    instruction.Opcode = unchecked(( FlowScriptOpcode )( -1 ));
                    instruction.OperandFloat = mReader.ReadSingle();
                }
                else
                {
                    instruction.Opcode = ( FlowScriptOpcode )mReader.ReadInt16();
                    instruction.OperandShort = mReader.ReadInt16();
                }

                instructions[i] = instruction;
            }

            return instructions;
        }

        public MessageScriptBinary ReadMessageScriptSection( ref FlowScriptBinarySectionHeader sectionHeader )
        {
            EnsureSectionHeaderInitialValidState( ref sectionHeader );

            if ( sectionHeader.ElementSize != sizeof( byte ) )
            {
                throw new InvalidDataException( $"{FlowScriptBinarySectionType.MessageScriptSection} unit size must be 1" );
            }

            if ( sectionHeader.ElementCount != 0 )
            {
                var bytes = mReader.ReadBytes( sectionHeader.ElementCount );
                using ( var memoryStream = new MemoryStream( bytes ) )
                {
                    return MessageScriptBinary.FromStream( memoryStream );
                }
            }
            else
            {
                return null;
            }
        }

        public byte[] ReadStringSection( ref FlowScriptBinarySectionHeader sectionHeader )
        {
            EnsureSectionHeaderInitialValidState( ref sectionHeader );

            if ( sectionHeader.ElementSize != sizeof( byte ) )
            {
                throw new InvalidDataException( $"{FlowScriptBinarySectionType.StringSection} unit size must be 1" );
            }

            return mReader.ReadBytes( sectionHeader.ElementCount );
        }

        public FlowScriptBinaryFormatVersion GetDetectedFormatVersion()
        {
            return mVersion;
        }

        public void Dispose()
        {
            if ( mDisposed )
                return;

            ( ( IDisposable )mReader ).Dispose();
            mDisposed = true;
        }

        private void ReadHeaderInternal( out FlowScriptBinaryHeader header )
        {
            // Check if the stream isn't too small to be a proper file
            if ( mReader.BaseStreamLength < FlowScriptBinaryHeader.SIZE )
            {
                throw new InvalidDataException( "Stream is too small to be valid" );
            }
            else
            {
                header = mReader.ReadStruct<FlowScriptBinaryHeader>();
                if ( !header.Magic.SequenceEqual( FlowScriptBinaryHeader.MAGIC ) )
                {
                    throw new InvalidDataException( "Header magic value does not match" );
                }
            }
        }

        private void MaybeSwapHeaderEndianness( ref FlowScriptBinaryHeader header )
        {
            // Swap endianness if high bits of section count are used
            if ( ( header.SectionCount & 0xFF000000 ) != 0 )
            {
                header = EndiannessHelper.Swap( header );

                if ( mReader.Endianness == Endianness.LittleEndian )
                {
                    mReader.Endianness = Endianness.BigEndian;
                    mVersion |= FlowScriptBinaryFormatVersion.BigEndian;
                }
                else
                {
                    mReader.Endianness = Endianness.LittleEndian;
                    mVersion ^= FlowScriptBinaryFormatVersion.BigEndian;
                }
            }
        }

        private void EnsureSectionHeaderInitialValidState( ref FlowScriptBinarySectionHeader sectionHeader )
        {
            if ( sectionHeader.FirstElementAddress == 0 )
            {
                throw new InvalidOperationException( "Section start offset is a null pointer" );
            }

            long absoluteAddress = mPositionBase + sectionHeader.FirstElementAddress;

            if ( !( absoluteAddress + ( sectionHeader.ElementSize * sectionHeader.ElementCount ) <= mReader.BaseStreamLength ) )
            {
                throw new InvalidDataException( "Stream is too small for the amount of data described. File is likely truncated" );
            }

            mReader.SeekBegin( absoluteAddress );
        }

        private void MaybeSwapVersionEndiannessByLabelSectionHeader( ref FlowScriptBinarySectionHeader sectionHeader )
        {
            if ( sectionHeader.ElementSize == FlowScriptBinaryLabel.SIZE_V1 && !mVersion.HasFlag( FlowScriptBinaryFormatVersion.Version1 ) )
            {
                mVersion = FlowScriptBinaryFormatVersion.Version1;
                if ( mReader.Endianness == Endianness.BigEndian )
                    mVersion |= FlowScriptBinaryFormatVersion.BigEndian;
            }
            else if ( sectionHeader.ElementSize == FlowScriptBinaryLabel.SIZE_V2 && !mVersion.HasFlag( FlowScriptBinaryFormatVersion.Version2 ) )
            {
                mVersion = FlowScriptBinaryFormatVersion.Version2;
                if ( mReader.Endianness == Endianness.BigEndian )
                    mVersion |= FlowScriptBinaryFormatVersion.BigEndian;
            }
            else if ( sectionHeader.ElementSize == FlowScriptBinaryLabel.SIZE_V3 && !mVersion.HasFlag( FlowScriptBinaryFormatVersion.Version3 ) )
            {
                mVersion = FlowScriptBinaryFormatVersion.Version3;
                if ( mReader.Endianness == Endianness.BigEndian )
                    mVersion |= FlowScriptBinaryFormatVersion.BigEndian;
            }
        }
    }
}