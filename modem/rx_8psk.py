#!/usr/bin/env python3
# -*- coding: utf-8 -*-

#
# SPDX-License-Identifier: GPL-3.0
#
# GNU Radio Python Flow Graph
# Title: 8PSK Modem  DJ0ABR
# Author: kurt
# Description: requires GNU Radio 3.8xxx
# GNU Radio version: 3.8.2.0

from gnuradio import analog
from gnuradio import audio
from gnuradio import blocks
from gnuradio import digital
from gnuradio import filter
from gnuradio.filter import firdes
from gnuradio import gr
import sys
import signal
from argparse import ArgumentParser
from gnuradio.eng_arg import eng_float, intx
from gnuradio import eng_notation


class rx_8psk(gr.top_block):

    def __init__(self, resamp=6, samp_rate=48000):
        gr.top_block.__init__(self, "8PSK Modem  DJ0ABR")

        ##################################################
        # Parameters
        ##################################################
        self.resamp = resamp
        self.samp_rate = samp_rate

        ##################################################
        # Variables
        ##################################################
        self.sps = sps = 4
        self.nfilts = nfilts = 32
        self.rrc_taps = rrc_taps = firdes.root_raised_cosine(nfilts, nfilts, 1.1/float(sps), 0.2, 11*sps*nfilts)
        self.outputsps = outputsps = 7
        self.mixf = mixf = 1500

        ##################################################
        # Blocks
        ##################################################
        self.mmse_resampler_xx_0_0 = filter.mmse_resampler_ff(0, samp_rate / 8000)
        self.mmse_resampler_xx_0 = filter.mmse_resampler_cc(0, resamp)
        self.low_pass_filter_0 = filter.fir_filter_fff(
            1,
            firdes.low_pass(
                12,
                samp_rate,
                3900,
                3300,
                firdes.WIN_HAMMING,
                6.76))
        self.digital_pfb_clock_sync_xxx_0 = digital.pfb_clock_sync_ccf(sps, 0.06, rrc_taps, nfilts, nfilts/16, 2, outputsps)
        self.digital_lms_dd_equalizer_cc_0 = digital.lms_dd_equalizer_cc(15, 0.01, outputsps, digital.constellation_8psk_natural().base())
        self.digital_diff_decoder_bb_0 = digital.diff_decoder_bb(8)
        self.digital_costas_loop_cc_0 = digital.costas_loop_cc(0.15, 8, False)
        self.digital_constellation_decoder_cb_0 = digital.constellation_decoder_cb(digital.constellation_8psk_natural().base())
        self.blocks_udp_sink_0_0_0 = blocks.udp_sink(gr.sizeof_int*1, '127.0.0.1', 40137, 120, False)
        self.blocks_udp_sink_0_0 = blocks.udp_sink(gr.sizeof_int*1, '127.0.0.1', 40136, 120, False)
        self.blocks_udp_sink_0 = blocks.udp_sink(gr.sizeof_char*1, '127.0.0.1', 40135, 344, False)
        self.blocks_multiply_xx_0_1_0 = blocks.multiply_vff(1)
        self.blocks_multiply_xx_0_1 = blocks.multiply_vff(1)
        self.blocks_multiply_xx_0_0_0 = blocks.multiply_vff(1)
        self.blocks_interleave_0_0 = blocks.interleave(gr.sizeof_int*1, 1)
        self.blocks_interleave_0 = blocks.interleave(gr.sizeof_int*1, 1)
        self.blocks_float_to_int_0_1 = blocks.float_to_int(1, 1)
        self.blocks_float_to_int_0_0 = blocks.float_to_int(1, 16777216)
        self.blocks_float_to_int_0 = blocks.float_to_int(1, 16777216)
        self.blocks_float_to_complex_0 = blocks.float_to_complex(1)
        self.blocks_complex_to_float_1 = blocks.complex_to_float(1)
        self.blocks_complex_to_float_0 = blocks.complex_to_float(1)
        self.audio_source_0 = audio.source(samp_rate, '', True)
        self.analog_sig_source_x_0_0_0 = analog.sig_source_c(samp_rate, analog.GR_COS_WAVE, mixf, 1, 0, 0)
        self.analog_const_source_x_0_1 = analog.sig_source_f(0, analog.GR_CONST_WAVE, 0, 0, 16777216)
        self.analog_const_source_x_0_0 = analog.sig_source_i(0, analog.GR_CONST_WAVE, 0, 0, 1000)
        self.analog_const_source_x_0 = analog.sig_source_i(0, analog.GR_CONST_WAVE, 0, 0, 1000)
        self.analog_agc2_xx_0_0 = analog.agc2_cc(1e-2, 0.2, 1, 2)
        self.analog_agc2_xx_0_0.set_max_gain(3)



        ##################################################
        # Connections
        ##################################################
        self.connect((self.analog_agc2_xx_0_0, 0), (self.digital_costas_loop_cc_0, 0))
        self.connect((self.analog_const_source_x_0, 0), (self.blocks_interleave_0, 0))
        self.connect((self.analog_const_source_x_0_0, 0), (self.blocks_interleave_0_0, 0))
        self.connect((self.analog_const_source_x_0_1, 0), (self.blocks_multiply_xx_0_1_0, 1))
        self.connect((self.analog_sig_source_x_0_0_0, 0), (self.blocks_complex_to_float_1, 0))
        self.connect((self.audio_source_0, 0), (self.low_pass_filter_0, 0))
        self.connect((self.audio_source_0, 0), (self.mmse_resampler_xx_0_0, 0))
        self.connect((self.blocks_complex_to_float_0, 0), (self.blocks_float_to_int_0, 0))
        self.connect((self.blocks_complex_to_float_0, 1), (self.blocks_float_to_int_0_0, 0))
        self.connect((self.blocks_complex_to_float_1, 1), (self.blocks_multiply_xx_0_0_0, 1))
        self.connect((self.blocks_complex_to_float_1, 0), (self.blocks_multiply_xx_0_1, 1))
        self.connect((self.blocks_float_to_complex_0, 0), (self.mmse_resampler_xx_0, 0))
        self.connect((self.blocks_float_to_int_0, 0), (self.blocks_interleave_0_0, 1))
        self.connect((self.blocks_float_to_int_0_0, 0), (self.blocks_interleave_0_0, 2))
        self.connect((self.blocks_float_to_int_0_1, 0), (self.blocks_interleave_0, 1))
        self.connect((self.blocks_interleave_0, 0), (self.blocks_udp_sink_0_0, 0))
        self.connect((self.blocks_interleave_0_0, 0), (self.blocks_udp_sink_0_0_0, 0))
        self.connect((self.blocks_multiply_xx_0_0_0, 0), (self.blocks_float_to_complex_0, 0))
        self.connect((self.blocks_multiply_xx_0_1, 0), (self.blocks_float_to_complex_0, 1))
        self.connect((self.blocks_multiply_xx_0_1_0, 0), (self.blocks_float_to_int_0_1, 0))
        self.connect((self.digital_constellation_decoder_cb_0, 0), (self.digital_diff_decoder_bb_0, 0))
        self.connect((self.digital_costas_loop_cc_0, 0), (self.blocks_complex_to_float_0, 0))
        self.connect((self.digital_costas_loop_cc_0, 0), (self.digital_constellation_decoder_cb_0, 0))
        self.connect((self.digital_diff_decoder_bb_0, 0), (self.blocks_udp_sink_0, 0))
        self.connect((self.digital_lms_dd_equalizer_cc_0, 0), (self.analog_agc2_xx_0_0, 0))
        self.connect((self.digital_pfb_clock_sync_xxx_0, 0), (self.digital_lms_dd_equalizer_cc_0, 0))
        self.connect((self.low_pass_filter_0, 0), (self.blocks_multiply_xx_0_0_0, 0))
        self.connect((self.low_pass_filter_0, 0), (self.blocks_multiply_xx_0_1, 0))
        self.connect((self.mmse_resampler_xx_0, 0), (self.digital_pfb_clock_sync_xxx_0, 0))
        self.connect((self.mmse_resampler_xx_0_0, 0), (self.blocks_multiply_xx_0_1_0, 0))


    def get_resamp(self):
        return self.resamp

    def set_resamp(self, resamp):
        self.resamp = resamp
        self.mmse_resampler_xx_0.set_resamp_ratio(self.resamp)

    def get_samp_rate(self):
        return self.samp_rate

    def set_samp_rate(self, samp_rate):
        self.samp_rate = samp_rate
        self.analog_sig_source_x_0_0_0.set_sampling_freq(self.samp_rate)
        self.low_pass_filter_0.set_taps(firdes.low_pass(12, self.samp_rate, 3900, 3300, firdes.WIN_HAMMING, 6.76))
        self.mmse_resampler_xx_0_0.set_resamp_ratio(self.samp_rate / 8000)

    def get_sps(self):
        return self.sps

    def set_sps(self, sps):
        self.sps = sps
        self.set_rrc_taps(firdes.root_raised_cosine(self.nfilts, self.nfilts, 1.1/float(self.sps), 0.2, 11*self.sps*self.nfilts))

    def get_nfilts(self):
        return self.nfilts

    def set_nfilts(self, nfilts):
        self.nfilts = nfilts
        self.set_rrc_taps(firdes.root_raised_cosine(self.nfilts, self.nfilts, 1.1/float(self.sps), 0.2, 11*self.sps*self.nfilts))

    def get_rrc_taps(self):
        return self.rrc_taps

    def set_rrc_taps(self, rrc_taps):
        self.rrc_taps = rrc_taps
        self.digital_pfb_clock_sync_xxx_0.update_taps(self.rrc_taps)

    def get_outputsps(self):
        return self.outputsps

    def set_outputsps(self, outputsps):
        self.outputsps = outputsps

    def get_mixf(self):
        return self.mixf

    def set_mixf(self, mixf):
        self.mixf = mixf
        self.analog_sig_source_x_0_0_0.set_frequency(self.mixf)




def argument_parser():
    description = 'requires GNU Radio 3.8xxx'
    parser = ArgumentParser(description=description)
    parser.add_argument(
        "-r", "--resamp", dest="resamp", type=intx, default=6,
        help="Set resamp [default=%(default)r]")
    parser.add_argument(
        "-s", "--samp-rate", dest="samp_rate", type=intx, default=48000,
        help="Set samp_rate [default=%(default)r]")
    return parser


def main(top_block_cls=rx_8psk, options=None):
    if options is None:
        options = argument_parser().parse_args()
    tb = top_block_cls(resamp=options.resamp, samp_rate=options.samp_rate)

    def sig_handler(sig=None, frame=None):
        tb.stop()
        tb.wait()

        sys.exit(0)

    signal.signal(signal.SIGINT, sig_handler)
    signal.signal(signal.SIGTERM, sig_handler)

    tb.start()

    tb.wait()


if __name__ == '__main__':
    main()
