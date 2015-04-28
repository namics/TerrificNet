(function ($) {
	"use strict";
	/**
	 * Nav module implementation.
	 *
	 * @author Denis Skeledzic-Gemperli <denis.skeledzic@namics.com>
	 * @namespace Tc.Module
	 * @class Nav
	 * @extends Tc.Module
	 */
	Tc.Module.Nav = Tc.Module.extend({

		init: function ($ctx, sandbox, modId) {
			this._super($ctx, sandbox, modId);
		},

		on: function (callback) {
			var mod = this,
				$ctx = mod.$ctx,

				$link = $ctx.find('.nav-main li a'),
				$flyout = $ctx.find('.nav-main-flyout'),
				$close = $ctx.find('.js-nav-close');

			$link.on('click', function () {
				var $this = $(this);
				$this.toggleClass('state-active');
				$this.next($flyout).toggleClass('state-open');
				$ctx.find('.state-active').not($this).toggleClass('state-active').next().toggleClass('state-open');
			});

			$close.on('click', function () {
				$link.removeClass('state-active');
				$flyout.removeClass('state-open');
			});


			callback();
		}
	});
}(Tc.$));
